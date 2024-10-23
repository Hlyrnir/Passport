using Mediator;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;
using Passport.Abstraction.Authentication;
using Passport.Application.Command.PassportVisa.Update;
using Passport.Application.Default;
using Passport.Application.Result;
using Passport.Contract.v01.Request.PassportVisa;

namespace Passport.Api.Endpoint.PassportVisa
{
    internal static class UpdatePassportVisaEndpoint
    {
        public const string Name = "UpdatePassportVisa";

        public static void AddUpdatePassportVisaEndpoint(this IEndpointRouteBuilder epBuilder, params string[] sPolicyName)
        {
            epBuilder.MapPut(
                EndpointRoute.PassportVisa.Update, UpdatePassportVisa)
                .RequireAuthorization(sPolicyName)
                .WithName(Name)
                .WithTags("PassportVisa")
                .Produces(StatusCodes.Status401Unauthorized)
                .Produces(StatusCodes.Status403Forbidden)
                .Produces<bool>(StatusCodes.Status200OK)
                .Produces<string>(StatusCodes.Status400BadRequest)
                .WithApiVersionSet(EndpointVersion.VersionSet!)
                .HasApiVersion(1.0);
        }

        private static async Task<IResult> UpdatePassportVisa(
            UpdatePassportVisaRequest rqstPassportVisa,
            IPassportCredential ppCredential,
            HttpContext httpContext,
            ISender mdtMediator,
            CancellationToken tknCancellation)
        {
            Guid guPassportId = Guid.Empty;

            if (httpContext.TryParsePassportId(out guPassportId) == false)
                return Results.BadRequest("Passport could not be identified.");

            UpdatePassportVisaCommand cmdUpdate = rqstPassportVisa.MapToCommand(guPassportId, ppCredential);

            MessageResult<bool> mdtResult = await mdtMediator.Send(cmdUpdate, tknCancellation);

            return mdtResult.Match(
                msgError =>
                {
                    if (msgError.Equals(AuthorizationError.PassportVisa.VisaDoesNotExist) == true)
                        return Results.Forbid();

                    return Results.BadRequest($"{msgError.Code}: {msgError.Description}");
                },
                bResult => TypedResults.Ok(bResult));
        }

        private static UpdatePassportVisaCommand MapToCommand(this UpdatePassportVisaRequest rqstPassportVisa, Guid guPassportId, IPassportCredential ppCredential)
        {
            ppCredential.Initialize(
                sProvider: rqstPassportVisa.ProviderToVerify,
                sCredential: rqstPassportVisa.CredentialToVerify,
                sSignature: rqstPassportVisa.SignatureToVerify);

            return new UpdatePassportVisaCommand()
            {
                PassportVisaId = rqstPassportVisa.PassportVisaId,
                ConcurrencyStamp = rqstPassportVisa.ConcurrencyStamp,
                Name = rqstPassportVisa.Name,
                Level = rqstPassportVisa.Level,
                CredentialToVerify = ppCredential,
                RestrictedPassportId = guPassportId
            };
        }
    }
}