using Mediator;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;
using Passport.Abstraction.Result;
using Passport.Application.Command.Authentication.ByRefreshToken;
using Passport.Application.Transfer;
using Passport.Contract.v01.Request.Authentication;
using Passport.Contract.v01.Response.Authentication;
using System.Threading;
using System.Threading.Tasks;

namespace Passport.Api.Endpoint.Authentication
{
    internal static class GenerateAuthenticationTokenByRefreshTokenEndpoint
    {
        public const string Name = "GenerateTokenByRefreshToken";

        public static void AddGenerateAuthenticationTokenByRefreshTokenEndpoint(this IEndpointRouteBuilder epBuilder)
        {
            epBuilder.MapPost(
                EndpointRoute.Authentication.RefreshToken, GenerateTokenByRefreshToken)
                .AllowAnonymous()
                .WithName(Name)
                .WithTags("Authentication")
                .Produces(StatusCodes.Status401Unauthorized)
                .Produces<AuthenticationTokenResponse>(StatusCodes.Status200OK)
                .Produces<string>(StatusCodes.Status400BadRequest)
                .WithApiVersionSet(EndpointVersion.VersionSet!)
                .HasApiVersion(1.0);
        }

        private static async Task<IResult> GenerateTokenByRefreshToken(
            GenerateAuthenticationTokenByRefreshTokenRequest rqstTokenByRefreshToken,
            HttpContext httpContext,
            ISender mdtMediator,
            CancellationToken tknCancellation)
        {
            AuthenticationTokenByRefreshTokenCommand cmdInsert = rqstTokenByRefreshToken.MapToCommand();

            IMessageResult<AuthenticationTokenTransferObject> mdtResult = await mdtMediator.Send(cmdInsert, tknCancellation);

            return mdtResult.Match(
                msgError => Results.BadRequest($"{msgError.Code}: {msgError.Description}"),
                dtoJwtToken => TypedResults.Ok(dtoJwtToken.MapToResponse()));
        }

        private static AuthenticationTokenByRefreshTokenCommand MapToCommand(this GenerateAuthenticationTokenByRefreshTokenRequest cmdRequest)
        {
            return new AuthenticationTokenByRefreshTokenCommand()
            {
                PassportId = cmdRequest.PassportId,
                Provider = cmdRequest.Provider,
                RefreshToken = cmdRequest.RefreshToken
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
