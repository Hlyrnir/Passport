using Mediator;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;
using Passport.Abstraction.Result;
using Passport.Application.Command.PassportHolder.ConfirmEmailAddress;
using Passport.Application.Default;
using Passport.Contract.v01.Request.PassportHolder;

namespace Passport.Api.Endpoint.PassportHolder
{
    internal static class ConfirmEmailAddressEndpoint
    {
        public const string Name = "ConfirmEmailAddress";

        public static void AddConfirmEmailAddressEndpoint(this IEndpointRouteBuilder epBuilder, params string[] sPolicyName)
        {
            epBuilder.MapPut(
                EndpointRoute.PassportHolder.ConfirmEmailAddress, ConfirmEmailAddress)
                .RequireAuthorization(sPolicyName)
                .WithName(Name)
                .WithTags("PassportHolder")
                .Produces(StatusCodes.Status401Unauthorized)
                .Produces(StatusCodes.Status403Forbidden)
                .Produces<bool>(StatusCodes.Status200OK)
                .Produces<string>(StatusCodes.Status400BadRequest)
                .WithApiVersionSet(EndpointVersion.VersionSet!)
                .HasApiVersion(1.0);
        }

        private static async Task<IResult> ConfirmEmailAddress(
            ConfirmEmailAddressRequest rqstConfirmEmailAddress,
            HttpContext httpContext,
            ISender mdtMediator,
            CancellationToken tknCancellation)
        {
            Guid guPassportId = Guid.Empty;

            if (httpContext.TryParsePassportId(out guPassportId) == false)
                return Results.BadRequest("Passport could not be identified.");

            ConfirmEmailAddressCommand cmdUpdate = rqstConfirmEmailAddress.MapToCommand(guPassportId);

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

        private static ConfirmEmailAddressCommand MapToCommand(this ConfirmEmailAddressRequest rqstConfirmEmailAddress, Guid guPassportId)
        {
            return new ConfirmEmailAddressCommand()
            {
                RestrictedPassportId = guPassportId,
                ConcurrencyStamp = rqstConfirmEmailAddress.ConcurrencyStamp,
                EmailAddress = rqstConfirmEmailAddress.EmailAddress,
                PassportHolderId = rqstConfirmEmailAddress.PassportHolderId
            };
        }
    }
}