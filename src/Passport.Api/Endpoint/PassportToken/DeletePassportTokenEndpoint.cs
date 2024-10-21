using Mediator;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Routing;
using Passport.Abstraction.Authentication;
using Passport.Abstraction.Result;
using Passport.Application.Command.PassportToken.Delete;
using Passport.Application.Default;
using Passport.Contract.v01.Request.PassportToken;

namespace Passport.Api.Endpoint.PassportToken
{
    internal static class DeletePassportTokenEndpoint
    {
        public const string Name = "DeletePassportToken";

        public static void AddDeletePassportTokenEndpoint(this IEndpointRouteBuilder epBuilder)
        {
            epBuilder.MapDelete(
                EndpointRoute.PassportToken.Delete, DeletePassportToken)
                .RequireAuthorization(EndpointAuthorization.Passport)
                .WithName(Name)
                .WithTags("PassportToken")
                .Produces(StatusCodes.Status401Unauthorized)
                .Produces(StatusCodes.Status403Forbidden)
                .Produces<bool>(StatusCodes.Status200OK)
                .Produces<string>(StatusCodes.Status400BadRequest)
                .WithApiVersionSet(EndpointVersion.VersionSet!)
                .HasApiVersion(1.0);
        }

        private static async Task<IResult> DeletePassportToken(
            [FromBody] DeletePassportTokenRequest rqstPassportVisa,
            IPassportCredential ppCredentialToRemove,
            IPassportCredential ppCredentialToVerify,
            HttpContext httpContext,
            ISender mdtMediator,
            CancellationToken tknCancellation)
        {
            Guid guPassportId = Guid.Empty;

            if (httpContext.TryParsePassportId(out guPassportId) == false)
                return Results.BadRequest("Passport could not be identified.");

            DeletePassportTokenCommand cmdUpdate = rqstPassportVisa.MapToCommand(guPassportId, ppCredentialToRemove, ppCredentialToVerify);

            IMessageResult<bool> mdtResult = await mdtMediator.Send(cmdUpdate, tknCancellation);

            return mdtResult.Match(
                msgError =>
                {
                    if (msgError.Equals(AuthorizationError.PassportVisa.VisaDoesNotExist) == true)
                        return Results.Forbid();

                    return Results.BadRequest($"{msgError.Code}: {msgError.Description}");
                },
                bResult => TypedResults.Ok(bResult));
        }

        private static DeletePassportTokenCommand MapToCommand(this DeletePassportTokenRequest rqstPassportToken, Guid guPassportId, IPassportCredential ppCredentialToRemove, IPassportCredential ppCredentialToVerify)
        {
            ppCredentialToRemove.Initialize(
                sProvider: rqstPassportToken.ProviderToRemove,
                sCredential: rqstPassportToken.CredentialToRemove,
                sSignature: rqstPassportToken.SignatureToRemove);

            ppCredentialToVerify.Initialize(
                sProvider: rqstPassportToken.ProviderToVerify,
                sCredential: rqstPassportToken.CredentialToVerify,
                sSignature: rqstPassportToken.SignatureToVerify);

            return new DeletePassportTokenCommand()
            {
                CredentialToRemove = ppCredentialToRemove,
                CredentialToVerify = ppCredentialToVerify,
                RestrictedPassportId = guPassportId
            };
        }
    }
}