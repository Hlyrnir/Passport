using Mediator;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;
using Passport.Abstraction.Authentication;
using Passport.Abstraction.Result;
using Passport.Application.Command.Authentication.ByCredential;
using Passport.Application.Transfer;
using Passport.Contract.v01.Request.Authentication;
using Passport.Contract.v01.Response.Authentication;
using System.Threading;
using System.Threading.Tasks;

namespace Passport.Api.Endpoint.Authentication
{
    internal static class GenerateAuthenticationTokenByCredentialEndpoint
    {
        public const string Name = "GenerateTokenByCredential";

        public static void AddGenerateAuthenticationTokenByCredentialEndpoint(this IEndpointRouteBuilder epBuilder, string sCorsPolicyName)
        {
            epBuilder.MapPost(
                EndpointRoute.Authentication.Token, GenerateTokenByCredential)
                .RequireCors(sCorsPolicyName)
                .AllowAnonymous()
                .WithName(Name)
                .WithTags("Authentication")
                .Produces(StatusCodes.Status401Unauthorized)
                .Produces<AuthenticationTokenResponse>(StatusCodes.Status200OK)
                .Produces<string>(StatusCodes.Status400BadRequest)
                .WithApiVersionSet(EndpointVersion.VersionSet!)
                .HasApiVersion(1.0);
        }

        private static async Task<IResult> GenerateTokenByCredential(
            GenerateAuthenticationTokenByCredentialRequest rqstTokenByCredential,
            IPassportCredential ppCredential,
            HttpContext httpContext,
            ISender mdtMediator,
            CancellationToken tknCancellation)
        {
            AuthenticationTokenByCredentialCommand cmdToken = rqstTokenByCredential.MapToCommand(ppCredential);

            IMessageResult<AuthenticationTokenTransferObject> mdtResult = await mdtMediator.Send(cmdToken, tknCancellation);

            return mdtResult.Match(
                msgError => Results.BadRequest($"{msgError.Code}: {msgError.Description}"),
                dtoJwtToken => TypedResults.Ok(dtoJwtToken.MapToResponse()));
        }

        private static AuthenticationTokenByCredentialCommand MapToCommand(this GenerateAuthenticationTokenByCredentialRequest cmdRequest, IPassportCredential ppCredential)
        {
            ppCredential.Initialize(
                    sProvider: cmdRequest.Provider,
                    sCredential: cmdRequest.Credential,
                    sSignature: cmdRequest.Signature);

            return new AuthenticationTokenByCredentialCommand()
            {
                Credential = ppCredential
            };
        }

        private static AuthenticationTokenResponse MapToResponse(this AuthenticationTokenTransferObject dtoJwtToken)
        {
            return new AuthenticationTokenResponse()
            {
                ExpiredAt = dtoJwtToken.ExpiredAt,
                Provider = dtoJwtToken.Provider,
                RefreshToken = dtoJwtToken.RefreshToken,
                Token = dtoJwtToken.Token
            };
        }
    }
}
