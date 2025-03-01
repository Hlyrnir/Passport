using Mediator;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;
using Passport.Abstraction.Authentication;
using Passport.Abstraction.Result;
using Passport.Application.Command.Authentication.Reset;
using Passport.Contract.v01.Request.Authentication;
using Passport.Contract.v01.Response.Authentication;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace Passport.Api.Endpoint.Authentication
{
    internal static class ResetRefreshTokenByPassportIdEndpoint
    {
        public const string Name = "ResetRefreshTokenByPassportId";

        public static void AddResetRefreshTokenByPassportIdEndpoint(this IEndpointRouteBuilder epBuilder, string sCorsPolicyName, params string[] sAuthorizationPolicyName)
        {
            epBuilder.MapPost(
                EndpointRoute.Authentication.Reset, ResetRefreshTokenByPassportId)
                .RequireCors(sCorsPolicyName)
                .RequireAuthorization(sAuthorizationPolicyName)
                .WithName(Name)
                .WithTags("Authentication")
                .Produces(StatusCodes.Status401Unauthorized)
                .Produces<AuthenticationTokenResponse>(StatusCodes.Status200OK)
                .Produces<string>(StatusCodes.Status400BadRequest)
                .WithApiVersionSet(EndpointVersion.VersionSet!)
                .HasApiVersion(1.0);
        }

        private static async Task<IResult> ResetRefreshTokenByPassportId(
            ResetRefreshTokenByPassportIdRequest rqstToken,
            IPassportCredential ppCredential,
            HttpContext httpContext,
            ISender mdtMediator,
            CancellationToken tknCancellation)
        {
            Guid guPassportId = Guid.Empty;

            if (httpContext.TryParsePassportId(out guPassportId) == false)
                return Results.BadRequest("Passport could not be identified.");

            ResetRefreshTokenByPassportIdCommand cmdToken = rqstToken.MapToCommand(guPassportId);

            IMessageResult<bool> mdtResult = await mdtMediator.Send(cmdToken, tknCancellation);

            return mdtResult.Match(
                msgError => Results.BadRequest($"{msgError.Code}: {msgError.Description}"),
                bResult => TypedResults.Ok(bResult));
        }

        private static ResetRefreshTokenByPassportIdCommand MapToCommand(this ResetRefreshTokenByPassportIdRequest cmdRequest, Guid guPassportId)
        {
            return new ResetRefreshTokenByPassportIdCommand()
            {
                PassportId = cmdRequest.PassportId,
                Provider = cmdRequest.Provider,
                RestrictedPassportId = guPassportId
            };
        }
    }
}
