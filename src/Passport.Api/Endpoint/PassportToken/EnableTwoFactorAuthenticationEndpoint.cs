using Mediator;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;
using Passport.Abstraction.Authentication;
using Passport.Abstraction.Result;
using Passport.Application.Command.PassportToken.EnableTwoFactorAuthentication;
using Passport.Application.Default;
using Passport.Contract.v01.Request.PassportToken;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace Passport.Api.Endpoint.PassportToken
{
    internal static class EnableTwoFactorAuthenticationEndpoint
    {
        public const string Name = "EnableTwoFactorAuthentication";

        public static void AddEnableTwoFactorAuthenticationEndpoint(this IEndpointRouteBuilder epBuilder, params string[] sPolicyName)
        {
            epBuilder.MapPut(
                EndpointRoute.PassportToken.TwoFactorAuthentication, EnableTwoFactorAuthentication)
                .RequireAuthorization(sPolicyName)
                .WithName(Name)
                .WithTags("PassportToken")
                .Produces(StatusCodes.Status401Unauthorized)
                .Produces(StatusCodes.Status403Forbidden)
                .Produces(StatusCodes.Status200OK)
                .Produces<string>(StatusCodes.Status400BadRequest)
                .WithApiVersionSet(EndpointVersion.VersionSet!)
                .HasApiVersion(1.0);
        }

        private static async Task<IResult> EnableTwoFactorAuthentication(
            EnableTwoFactorAuthenticationRequest rqstPassportToken,
            IPassportCredential ppCredentialToVerify,
            HttpContext httpContext,
            ISender mdtMediator,
            CancellationToken tknCancellation)
        {
            Guid guPassportId = Guid.Empty;

            if (httpContext.TryParsePassportId(out guPassportId) == false)
                return Results.BadRequest("Passport could not be identified.");

            EnableTwoFactorAuthenticationCommand cmdInsert = rqstPassportToken.MapToCommand(guPassportId, ppCredentialToVerify);

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

        private static EnableTwoFactorAuthenticationCommand MapToCommand(this EnableTwoFactorAuthenticationRequest rqstTwoFactorAuthentication, Guid guPassportId, IPassportCredential ppCredentialToVerify)
        {
            ppCredentialToVerify.Initialize(
                sProvider: rqstTwoFactorAuthentication.ProviderToVerify,
                sCredential: rqstTwoFactorAuthentication.CredentialToVerify,
                sSignature: rqstTwoFactorAuthentication.SignatureToVerify);

            return new EnableTwoFactorAuthenticationCommand()
            {
                TwoFactorIsEnabled=rqstTwoFactorAuthentication.TwoFactorIsEnabled,
                CredentialToVerify = ppCredentialToVerify,
                RestrictedPassportId = guPassportId
            };
        }
    }
}
