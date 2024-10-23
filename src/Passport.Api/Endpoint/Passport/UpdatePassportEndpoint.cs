using Mediator;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;
using Passport.Abstraction.Authentication;
using Passport.Abstraction.Result;
using Passport.Application.Command.Passport.Update;
using Passport.Application.Default;
using Passport.Contract.v01.Request.Passport;

namespace Passport.Api.Endpoint.Passport
{
    internal static class UpdatePassportEndpoint
    {
        public const string Name = "UpdatePassport";

        public static void AddUpdatePassportEndpoint(this IEndpointRouteBuilder epBuilder, params string[] sPolicyName)
        {
            epBuilder.MapPut(
                EndpointRoute.Passport.Update, UpdatePassport)
                .RequireAuthorization(sPolicyName)
                .WithName(Name)
                .WithTags("Passport")
                .Produces(StatusCodes.Status401Unauthorized)
                .Produces(StatusCodes.Status403Forbidden)
                .Produces<bool>(StatusCodes.Status200OK)
                .Produces<string>(StatusCodes.Status400BadRequest)
                .WithApiVersionSet(EndpointVersion.VersionSet!)
                .HasApiVersion(1.0);
        }

        private static async Task<IResult> UpdatePassport(
            UpdatePassportRequest rqstPassport,
            IPassportCredential ppCredential,
            HttpContext httpContext,
            ISender mdtMediator,
            CancellationToken tknCancellation)
        {
            Guid guPassportId = Guid.Empty;

            if (httpContext.TryParsePassportId(out guPassportId) == false)
                return Results.BadRequest("Passport could not be identified.");

            UpdatePassportCommand cmdUpdate = rqstPassport.MapToCommand(guPassportId, ppCredential);

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

        private static UpdatePassportCommand MapToCommand(this UpdatePassportRequest rqstPassport, Guid guPassportId, IPassportCredential ppCredential)
        {
            ppCredential.Initialize(
                sProvider: rqstPassport.ProviderToVerify,
                sCredential: rqstPassport.CredentialToVerify,
                sSignature: rqstPassport.SignatureToVerify);

            return new UpdatePassportCommand()
            {
                PassportIdToUpdate = rqstPassport.PassportId,
                ConcurrencyStamp = rqstPassport.ConcurrencyStamp,
                ExpiredAt = rqstPassport.ExpiredAt,
                IsAuthority = rqstPassport.IsAuthority,
                IsEnabled = rqstPassport.IsEnabled,
                LastCheckedAt = rqstPassport.LastCheckedAt,
                LastCheckedBy = rqstPassport.LastCheckedBy,
                PassportVisaId = rqstPassport.PassportVisa,
                CredentialToVerify = ppCredential,
                RestrictedPassportId = guPassportId
            };
        }
    }
}