using Mediator;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;
using Passport.Application.Default;
using Passport.Application.Query.PassportVisa.ById;
using Passport.Application.Result;
using Passport.Contract.v01.Response.PassportVisa;

namespace Passport.Api.Endpoint.PassportVisa
{
    internal static class FindPassportVisaByIdEndpoint
    {
        public const string Name = "FindPassportVisaById";

        public static void AddFindPassportVisaByIdEndpoint(this IEndpointRouteBuilder epBuilder)
        {
            epBuilder.MapGet(
                EndpointRoute.PassportVisa.GetById, FindPassportVisaById)
                .RequireAuthorization(EndpointAuthorization.Passport)
                .WithName(Name)
                .WithTags("PassportVisa")
                .Produces(StatusCodes.Status401Unauthorized)
                .Produces(StatusCodes.Status403Forbidden)
                .Produces<PassportVisaResponse>(StatusCodes.Status200OK)
                .Produces<string>(StatusCodes.Status400BadRequest)
                .WithApiVersionSet(EndpointVersion.VersionSet!)
                .HasApiVersion(1.0);
        }

        private static async Task<IResult> FindPassportVisaById(
            Guid guPassportVisaIdToFind,
            HttpContext httpContext,
            ISender mdtMediator,
            CancellationToken tknCancellation)
        {
            Guid guPassportId = Guid.Empty;

            if (httpContext.TryParsePassportId(out guPassportId) == false)
                return Results.BadRequest("Passport could not be identified.");

            PassportVisaByIdQuery qryPassportVisa = MapToQuery(guPassportVisaIdToFind, guPassportId);

            MessageResult<PassportVisaByIdResult> mdtResult = await mdtMediator.Send(qryPassportVisa, tknCancellation);

            return mdtResult.Match(
                msgError =>
                {
                    if (msgError.Equals(AuthorizationError.PassportVisa.VisaDoesNotExist) == true)
                        return Results.Forbid();

                    return Results.BadRequest($"{msgError.Code}: {msgError.Description}");
                },
                ppPassportVisa =>
                {
                    PassportVisaResponse rspnPassportVisa = ppPassportVisa.MapToResponse();
                    return TypedResults.Ok(rspnPassportVisa);
                });
        }

        private static PassportVisaByIdQuery MapToQuery(Guid guPassportVisaIdToFind, Guid guPassportId)
        {
            return new PassportVisaByIdQuery()
            {
                RestrictedPassportId = guPassportId,
                PassportVisaId = guPassportVisaIdToFind
            };
        }

        private static PassportVisaResponse MapToResponse(this PassportVisaByIdResult rsltPassportVisa)
        {
            return new PassportVisaResponse()
            {
                ConcurrencyStamp = rsltPassportVisa.PassportVisa.ConcurrencyStamp,
                Id = rsltPassportVisa.PassportVisa.Id,
                Level = rsltPassportVisa.PassportVisa.Level,
                Name = rsltPassportVisa.PassportVisa.Name
            };
        }
    }
}