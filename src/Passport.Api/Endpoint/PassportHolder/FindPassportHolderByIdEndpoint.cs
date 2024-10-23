using Mediator;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;
using Passport.Application.Default;
using Passport.Application.Query.PassportHolder.ById;
using Passport.Application.Result;
using Passport.Contract.v01.Response.PassportHolder;

namespace Passport.Api.Endpoint.PassportHolder
{
    internal static class FindPassportHolderByIdEndpoint
    {
        public const string Name = "FindPassportHolderById";

        public static void AddFindPassportHolderByIdEndpoint(this IEndpointRouteBuilder epBuilder, params string[] sPolicyName)
        {
            epBuilder.MapGet(
                EndpointRoute.PassportHolder.GetById, FindPassportHolderById)
                .RequireAuthorization(sPolicyName)
                .WithName(Name)
                .WithTags("PassportHolder")
                .Produces(StatusCodes.Status401Unauthorized)
                .Produces(StatusCodes.Status403Forbidden)
                .Produces<PassportHolderResponse>(StatusCodes.Status200OK)
                .Produces<string>(StatusCodes.Status400BadRequest)
                .WithApiVersionSet(EndpointVersion.VersionSet!)
                .HasApiVersion(1.0);
        }

        private static async Task<IResult> FindPassportHolderById(
            Guid guPassportHolderIdToFind,
            HttpContext httpContext,
            ISender mdtMediator,
            CancellationToken tknCancellation)
        {
            Guid guPassportId = Guid.Empty;

            if (httpContext.TryParsePassportId(out guPassportId) == false)
                return Results.BadRequest("Passport could not be identified.");

            PassportHolderByIdQuery qryPassportHolder = MapToQuery(guPassportHolderIdToFind, guPassportId);

            MessageResult<PassportHolderByIdResult> mdtResult = await mdtMediator.Send(qryPassportHolder, tknCancellation);

            return mdtResult.Match(
                msgError =>
                {
                    if (msgError.Equals(AuthorizationError.PassportVisa.VisaDoesNotExist) == true)
                        return Results.Forbid();

                    return Results.BadRequest($"{msgError.Code}: {msgError.Description}");
                },
                ppPassportHolder =>
                {
                    PassportHolderResponse rspnPassportHolder = ppPassportHolder.MapToResponse();
                    return TypedResults.Ok(rspnPassportHolder);
                });
        }

        private static PassportHolderByIdQuery MapToQuery(Guid guPassportHolderIdToFind, Guid guPassportId)
        {
            return new PassportHolderByIdQuery()
            {
                RestrictedPassportId = guPassportId,
                PassportHolderId = guPassportHolderIdToFind
            };
        }

        private static PassportHolderResponse MapToResponse(this PassportHolderByIdResult rsltPassportHolder)
        {
            return new PassportHolderResponse()
            {
                PassportHolderId = rsltPassportHolder.PassportHolder.Id,
                ConcurrencyStamp = rsltPassportHolder.PassportHolder.ConcurrencyStamp,
                CultureName = rsltPassportHolder.PassportHolder.CultureName,
                EmailAddress = rsltPassportHolder.PassportHolder.EmailAddress,
                FirstName = rsltPassportHolder.PassportHolder.FirstName,
                LastName = rsltPassportHolder.PassportHolder.LastName,
                PhoneNumber = rsltPassportHolder.PassportHolder.PhoneNumber
            };
        }
    }
}