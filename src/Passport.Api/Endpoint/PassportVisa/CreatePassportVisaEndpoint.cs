using Mediator;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;
using Passport.Application.Command.PassportVisa.Create;
using Passport.Application.Default;
using Passport.Application.Result;
using Passport.Contract.v01.Request.PassportVisa;

namespace Passport.Api.Endpoint.PassportVisa
{
    internal static class CreatePassportVisaEndpoint
    {
        public const string Name = "CreatePassportVisa";

        public static void AddCreatePassportVisaEndpoint(this IEndpointRouteBuilder epBuilder)
        {
            epBuilder.MapPost(
                EndpointRoute.PassportVisa.Create, CreatePassportVisa)
                .RequireAuthorization(EndpointAuthorization.Passport)
                .WithName(Name)
                .WithTags("PassportVisa")
                .Produces(StatusCodes.Status401Unauthorized)
                .Produces(StatusCodes.Status403Forbidden)
                .Produces(StatusCodes.Status201Created)
                .Produces<string>(StatusCodes.Status400BadRequest)
                .WithApiVersionSet(EndpointVersion.VersionSet!)
                .HasApiVersion(1.0);
        }

        private static async Task<IResult> CreatePassportVisa(
            CreatePassportVisaRequest rqstPassportVisa,
            HttpContext httpContext,
            ISender mdtMediator,
            CancellationToken tknCancellation)
        {
            Guid guPassportId = Guid.Empty;

            if (httpContext.TryParsePassportId(out guPassportId) == false)
                return Results.BadRequest("Passport could not be identified.");

            CreatePassportVisaCommand cmdInsert = rqstPassportVisa.MapToCommand(guPassportId);

            MessageResult<Guid> mdtResult = await mdtMediator.Send(cmdInsert, tknCancellation);

            return mdtResult.Match(
                msgError =>
                {
                    if (msgError.Equals(AuthorizationError.PassportVisa.VisaDoesNotExist) == true)
                        return Results.Forbid();

                    return Results.BadRequest($"{msgError.Code}: {msgError.Description}");
                },
                guPassportVisaId => TypedResults.CreatedAtRoute(FindPassportVisaByIdEndpoint.Name, new { guId = guPassportVisaId }));
        }

        private static CreatePassportVisaCommand MapToCommand(this CreatePassportVisaRequest cmdRequest, Guid guPassportId)
        {
            return new CreatePassportVisaCommand()
            {
                RestrictedPassportId = guPassportId,
                Name = cmdRequest.Name,
                Level = cmdRequest.Level
            };
        }
    }
}
