using Mediator;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Routing;
using Passport.Abstraction.Authentication;
using Passport.Abstraction.Result;
using Passport.Application.Command.Passport.Seize;
using Passport.Application.Default;
using Passport.Contract.v01.Request.Passport;

namespace Passport.Api.Endpoint.Passport
{
    internal static class SeizePassportEndpoint
    {
        public const string Name = "SeizePassport";

        public static void AddSeizePassportEndpoint(this IEndpointRouteBuilder epBuilder)
        {
            epBuilder.MapDelete(
                EndpointRoute.Passport.Delete, SeizePassport)
                .RequireAuthorization(EndpointAuthorization.Passport)
                .WithName(Name)
                .WithTags("Passport")
                .Produces(StatusCodes.Status401Unauthorized)
                .Produces(StatusCodes.Status403Forbidden)
                .Produces<bool>(StatusCodes.Status200OK)
                .Produces<string>(StatusCodes.Status400BadRequest)
                .WithApiVersionSet(EndpointVersion.VersionSet!)
                .HasApiVersion(1.0);
        }

        private static async Task<IResult> SeizePassport(
            [FromBody] SeizePassportRequest rqstPassport,
            IPassportCredential ppCredential,
            HttpContext httpContext,
            ISender mdtMediator,
            CancellationToken tknCancellation)
        {
            Guid guPassportId = Guid.Empty;

            if (httpContext.TryParsePassportId(out guPassportId) == false)
                return Results.BadRequest("Passport could not be identified.");

            SeizePassportCommand cmdUpdate = rqstPassport.MapToCommand(guPassportId, ppCredential);

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

        private static SeizePassportCommand MapToCommand(this SeizePassportRequest rqstPassport, Guid guPassportId, IPassportCredential ppCredential)
        {
            ppCredential.Initialize(
                sProvider: rqstPassport.ProviderToVerify,
                sCredential: rqstPassport.CredentialToVerify,
                sSignature: rqstPassport.SignatureToVerify);

            return new SeizePassportCommand()
            {
                PassportIdToSeize = rqstPassport.PassportId,
                CredentialToVerify = ppCredential,
                RestrictedPassportId = guPassportId
            };
        }
    }
}