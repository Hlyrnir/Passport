using Mediator;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Routing;
using Passport.Abstraction.Authentication;
using Passport.Abstraction.Result;
using Passport.Application.Command.PassportHolder.Delete;
using Passport.Application.Default;
using Passport.Contract.v01.Request.PassportHolder;

namespace Passport.Api.Endpoint.PassportHolder
{
    internal static class DeletePassportHolderEndpoint
    {
        public const string Name = "DeletePassportHolder";

        public static void AddDeletePassportHolderEndpoint(this IEndpointRouteBuilder epBuilder)
        {
            epBuilder.MapDelete(
                EndpointRoute.PassportHolder.Delete, DeletePassportHolder)
                .RequireAuthorization(EndpointAuthorization.Passport)
                .WithName(Name)
                .WithTags("PassportHolder")
                .Produces(StatusCodes.Status401Unauthorized)
                .Produces(StatusCodes.Status403Forbidden)
                .Produces<bool>(StatusCodes.Status200OK)
                .Produces<string>(StatusCodes.Status400BadRequest)
                .WithApiVersionSet(EndpointVersion.VersionSet!)
                .HasApiVersion(1.0);
        }

        private static async Task<IResult> DeletePassportHolder(
            [FromBody] DeletePassportHolderRequest rqstPassportHolder,
            IPassportCredential ppCredential,
            HttpContext httpContext,
            ISender mdtMediator,
            CancellationToken tknCancellation)
        {
            Guid guPassportId = Guid.Empty;

            if (httpContext.TryParsePassportId(out guPassportId) == false)
                return Results.BadRequest("Passport could not be identified.");

            DeletePassportHolderCommand cmdUpdate = rqstPassportHolder.MapToCommand(guPassportId, ppCredential);

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

        private static DeletePassportHolderCommand MapToCommand(this DeletePassportHolderRequest rqstPassportHolder, Guid guPassportId, IPassportCredential ppCredential)
        {
            ppCredential.Initialize(
                sProvider: rqstPassportHolder.ProviderToVerify,
                sCredential: rqstPassportHolder.CredentialToVerify,
                sSignature: rqstPassportHolder.SignatureToVerify);

            return new DeletePassportHolderCommand()
            {
                PassportHolderId = rqstPassportHolder.PassportHolderId,
                CredentialToVerify = ppCredential,
                RestrictedPassportId = guPassportId
            };
        }
    }
}