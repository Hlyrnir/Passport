using Mediator;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;
using Passport.Abstraction.Result;
using Passport.Application.Command.PassportHolder.Update;
using Passport.Application.Default;
using Passport.Contract.v01.Request.PassportHolder;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace Passport.Api.Endpoint.PassportHolder
{
    internal static class UpdatePassportHolderEndpoint
    {
        public const string Name = "UpdatePassportHolder";

        public static void AddUpdatePassportHolderEndpoint(this IEndpointRouteBuilder epBuilder, string sCorsPolicyName, params string[] sAuthorizationPolicyName)
        {
            epBuilder.MapPut(
                EndpointRoute.PassportHolder.Update, UpdatePassportHolder)
                .RequireCors(sCorsPolicyName)
                .RequireAuthorization(sAuthorizationPolicyName)
                .WithName(Name)
                .WithTags("PassportHolder")
                .Produces(StatusCodes.Status401Unauthorized)
                .Produces(StatusCodes.Status403Forbidden)
                .Produces<bool>(StatusCodes.Status200OK)
                .Produces<string>(StatusCodes.Status400BadRequest)
                .WithApiVersionSet(EndpointVersion.VersionSet!)
                .HasApiVersion(1.0);
        }

        private static async Task<IResult> UpdatePassportHolder(
            UpdatePassportHolderRequest rqstPassportHolder,
            HttpContext httpContext,
            ISender mdtMediator,
            CancellationToken tknCancellation)
        {
            Guid guPassportId = Guid.Empty;

            if (httpContext.TryParsePassportId(out guPassportId) == false)
                return Results.BadRequest("Passport could not be identified.");

            UpdatePassportHolderCommand cmdUpdate = rqstPassportHolder.MapToCommand(guPassportId);

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

        private static UpdatePassportHolderCommand MapToCommand(this UpdatePassportHolderRequest rqstPassportHolder, Guid guPassportId)
        {
            return new UpdatePassportHolderCommand()
            {
                RestrictedPassportId = guPassportId,
                PassportHolderId = rqstPassportHolder.PassportHolderId,
                ConcurrencyStamp = rqstPassportHolder.ConcurrencyStamp,
                CultureName = rqstPassportHolder.CultureName,
                EmailAddress = rqstPassportHolder.EmailAddress,
                FirstName = rqstPassportHolder.FirstName,
                LastName = rqstPassportHolder.LastName,
                PhoneNumber = rqstPassportHolder.PhoneNumber
            };
        }
    }
}