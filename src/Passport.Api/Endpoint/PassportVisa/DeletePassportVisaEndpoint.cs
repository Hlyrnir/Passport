using Mediator;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Routing;
using Passport.Abstraction.Authentication;
using Passport.Application.Command.PassportVisa.Delete;
using Passport.Application.Default;
using Passport.Application.Result;
using Passport.Contract.v01.Request.PassportVisa;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace Passport.Api.Endpoint.PassportVisa
{
    internal static class DeletePassportVisaEndpoint
    {
        public const string Name = "DeletePassportVisa";

        public static void AddDeletePassportVisaEndpoint(this IEndpointRouteBuilder epBuilder, string sCorsPolicyName, params string[] sAuthorizationPolicyName)
        {
            epBuilder.MapDelete(
                EndpointRoute.PassportVisa.Delete, DeletePassportVisa)
                .RequireCors(sCorsPolicyName)
                .RequireAuthorization(sAuthorizationPolicyName)
                .WithName(Name)
                .WithTags("PassportVisa")
                .Produces(StatusCodes.Status401Unauthorized)
                .Produces(StatusCodes.Status403Forbidden)
                .Produces<bool>(StatusCodes.Status200OK)
                .Produces<string>(StatusCodes.Status400BadRequest)
                .WithApiVersionSet(EndpointVersion.VersionSet!)
                .HasApiVersion(1.0);
        }

        private static async Task<IResult> DeletePassportVisa(
            [FromBody] DeletePassportVisaRequest rqstPassportVisa,
            IPassportCredential ppCredential,
            HttpContext httpContext,
            ISender mdtMediator,
            CancellationToken tknCancellation)
        {
            Guid guPassportId = Guid.Empty;

            if (httpContext.TryParsePassportId(out guPassportId) == false)
                return Results.BadRequest("Passport could not be identified.");

            DeletePassportVisaCommand cmdUpdate = rqstPassportVisa.MapToCommand(guPassportId, ppCredential);

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

        private static DeletePassportVisaCommand MapToCommand(this DeletePassportVisaRequest rqstPassportVisa, Guid guPassportId, IPassportCredential ppCredential)
        {
            ppCredential.Initialize(
                sProvider: rqstPassportVisa.ProviderToVerify,
                sCredential: rqstPassportVisa.CredentialToVerify,
                sSignature: rqstPassportVisa.SignatureToVerify);

            return new DeletePassportVisaCommand()
            {
                PassportVisaId = rqstPassportVisa.PassportVisaId,
                CredentialToVerify = ppCredential,
                RestrictedPassportId = guPassportId
            };
        }
    }
}