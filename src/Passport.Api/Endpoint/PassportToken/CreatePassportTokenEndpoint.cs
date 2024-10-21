using Mediator;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Routing;
using Passport.Abstraction.Authentication;
using Passport.Abstraction.Result;
using Passport.Application.Command.PassportToken.Create;
using Passport.Application.Default;
using Passport.Contract.v01.Request.PassportToken;

namespace Passport.Api.Endpoint.PassportToken
{
    internal static class CreatePassportTokenEndpoint
    {
        public const string Name = "CreatePassportToken";

        public static void AddCreatePassportTokenEndpoint(this IEndpointRouteBuilder epBuilder)
        {
            epBuilder.MapPost(
                EndpointRoute.PassportToken.Create, CreatePassportToken)
                .RequireAuthorization(EndpointAuthorization.Passport)
                .WithName(Name)
                .WithTags("PassportToken")
                .Produces(StatusCodes.Status401Unauthorized)
                .Produces(StatusCodes.Status403Forbidden)
                .Produces(StatusCodes.Status201Created)
                .Produces<string>(StatusCodes.Status400BadRequest)
                .WithApiVersionSet(EndpointVersion.VersionSet!)
                .HasApiVersion(1.0);
        }

        private static async Task<IResult> CreatePassportToken(
            [FromBody]CreatePassportTokenRequest rqstPassportToken,
            IPassportCredential ppCredentialToAdd,
            IPassportCredential ppCredentialToVerify,
            HttpContext httpContext,
            ISender mdtMediator,
            CancellationToken tknCancellation)
        {
            Guid guPassportId = Guid.Empty;

            if (httpContext.TryParsePassportId(out guPassportId) == false)
                return Results.BadRequest("Passport could not be identified.");

            CreatePassportTokenCommand cmdInsert = rqstPassportToken.MapToCommand(guPassportId, ppCredentialToAdd, ppCredentialToVerify);

            IMessageResult<Guid> mdtResult = await mdtMediator.Send(cmdInsert, tknCancellation);

            return mdtResult.Match(
                msgError =>
                {
                    if (msgError.Equals(AuthorizationError.PassportVisa.VisaDoesNotExist) == true)
                        return Results.Forbid();

                    return Results.BadRequest($"{msgError.Code}: {msgError.Description}");
                },
                guPassportTokenId => TypedResults.Created());
        }

        private static CreatePassportTokenCommand MapToCommand(this CreatePassportTokenRequest rqstPassportToken, Guid guPassportId, IPassportCredential ppCredentialToAdd, IPassportCredential ppCredentialToVerify)
        {
            ppCredentialToAdd.Initialize(
                sProvider: rqstPassportToken.ProviderToAdd,
                sCredential: rqstPassportToken.CredentialToAdd,
                sSignature: rqstPassportToken.SignatureToAdd);

            ppCredentialToVerify.Initialize(
                sProvider: rqstPassportToken.ProviderToVerify,
                sCredential: rqstPassportToken.CredentialToVerify,
                sSignature: rqstPassportToken.SignatureToVerify);

            return new CreatePassportTokenCommand()
            {
                CredentialToAdd = ppCredentialToAdd,
                CredentialToVerify = ppCredentialToVerify,
                RestrictedPassportId = guPassportId
            };
        }
    }
}
