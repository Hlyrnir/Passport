using Mediator;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;
using Passport.Abstraction.Authentication;
using Passport.Abstraction.Result;
using Passport.Application.Command.PassportToken.ResetCredential;
using Passport.Application.Default;
using Passport.Contract.v01.Request.PassportToken;

namespace Passport.Api.Endpoint.PassportToken
{
    internal static class ResetCredentialEndpoint
    {
        public const string Name = "ResetCredential";

        public static void AddResetCredentialEndpoint(this IEndpointRouteBuilder epBuilder)
        {
            epBuilder.MapPut(
                EndpointRoute.PassportToken.ResetCredential, ResetCredential)
                .RequireAuthorization(EndpointAuthorization.Passport)
                .WithName(Name)
                .WithTags("PassportToken")
                .Produces(StatusCodes.Status401Unauthorized)
                .Produces(StatusCodes.Status403Forbidden)
                .Produces(StatusCodes.Status200OK)
                .Produces<string>(StatusCodes.Status400BadRequest)
                .WithApiVersionSet(EndpointVersion.VersionSet!)
                .HasApiVersion(1.0);
        }

        private static async Task<IResult> ResetCredential(
            ResetCredentialRequest rqstPassportToken,
            IPassportCredential ppCredentialToApply,
            IPassportCredential ppCredentialToVerify,
            HttpContext httpContext,
            ISender mdtMediator,
            CancellationToken tknCancellation)
        {
            Guid guPassportId = Guid.Empty;

            if (httpContext.TryParsePassportId(out guPassportId) == false)
                return Results.BadRequest("Passport could not be identified.");

            ResetCredentialCommand cmdInsert = rqstPassportToken.MapToCommand(guPassportId, ppCredentialToApply, ppCredentialToVerify);

            IMessageResult<bool> mdtResult = await mdtMediator.Send(cmdInsert, tknCancellation);

            return mdtResult.Match(
                msgError =>
                {
                    if (msgError.Equals(AuthorizationError.PassportVisa.VisaDoesNotExist) == true)
                        return Results.Forbid();

                    return Results.BadRequest($"{msgError.Code}: {msgError.Description}");
                },
                bResult => TypedResults.Ok(bResult));
        }

        private static ResetCredentialCommand MapToCommand(this ResetCredentialRequest rqstResetCredential, Guid guPassportId, IPassportCredential ppCredentialToApply, IPassportCredential ppCredentialToVerify)
        {
            ppCredentialToVerify.Initialize(
                sProvider: rqstResetCredential.ProviderToApply,
                sCredential: rqstResetCredential.CredentialToApply,
                sSignature: rqstResetCredential.SignatureToApply);

            ppCredentialToVerify.Initialize(
                sProvider: rqstResetCredential.ProviderToVerify,
                sCredential: rqstResetCredential.CredentialToVerify,
                sSignature: rqstResetCredential.SignatureToVerify);

            return new ResetCredentialCommand()
            {
                CredentialToApply = ppCredentialToApply,
                CredentialToVerify = ppCredentialToVerify,
                RestrictedPassportId = guPassportId
            };
        }
    }
}
