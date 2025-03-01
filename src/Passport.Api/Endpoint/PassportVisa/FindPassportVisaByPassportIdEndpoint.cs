using Mediator;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;
using Passport.Application.Default;
using Passport.Application.Query.PassportVisa.ByPassportId;
using Passport.Application.Result;
using Passport.Application.Transfer;
using Passport.Contract.v01.Response.PassportVisa;
using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace Passport.Api.Endpoint.PassportVisa
{
    internal static class FindPassportVisaByPassportIdEndpoint
    {
        public const string Name = "FindPassportVisaByPassportId";

        public static void AddFindPassportVisaByPassportIdEndpoint(this IEndpointRouteBuilder epBuilder, string sCorsPolicyName, params string[] sAuthorizationPolicyName)
        {
            epBuilder.MapGet(
                EndpointRoute.PassportVisa.GetByPassportId, FindPassportVisaByPassportId)
                .RequireCors(sCorsPolicyName)
                .RequireAuthorization(sAuthorizationPolicyName)
                .WithName(Name)
                .WithTags("PassportVisa")
                .Produces(StatusCodes.Status401Unauthorized)
                .Produces(StatusCodes.Status403Forbidden)
                .Produces<IEnumerable<PassportVisaResponse>>(StatusCodes.Status200OK)
                .Produces<string>(StatusCodes.Status400BadRequest)
                .WithApiVersionSet(EndpointVersion.VersionSet!)
                .HasApiVersion(1.0);
        }

        private static async Task<IResult> FindPassportVisaByPassportId(
            Guid guPassportIdToFind,
            HttpContext httpContext,
            ISender mdtMediator,
            CancellationToken tknCancellation)
        {
            Guid guId = Guid.Empty;

            if (httpContext.TryParsePassportId(out guId) == false)
                return Results.BadRequest("Passport could not be identified.");

            PassportVisaByPassportIdQuery qryPassportVisa = MapToQuery(guId, guPassportIdToFind);

            MessageResult<PassportVisaByPassportIdResult> mdtResult = await mdtMediator.Send(qryPassportVisa, tknCancellation);

            return mdtResult.Match(
                msgError =>
                {
                    if (msgError.Equals(AuthorizationError.PassportVisa.VisaDoesNotExist) == true)
                        return Results.Forbid();

                    return Results.BadRequest($"{msgError.Code}: {msgError.Description}");
                },
                rsltPassportVisa =>
                {
                    IEnumerable<PassportVisaResponse> rspnPassportVisa = rsltPassportVisa.MapToResponse();
                    return TypedResults.Ok(rspnPassportVisa);
                });
        }

        private static PassportVisaByPassportIdQuery MapToQuery(Guid guPassportId, Guid guPassportIdToFind)
        {
            return new PassportVisaByPassportIdQuery()
            {
                RestrictedPassportId = guPassportId,
                PassportIdToFind = guPassportIdToFind
            };
        }

        private static IEnumerable<PassportVisaResponse> MapToResponse(this PassportVisaByPassportIdResult rsltPassportVisa)
        {
            foreach (PassportVisaTransferObject ppVisa in rsltPassportVisa.PassportVisa)
            {
                yield return new PassportVisaResponse()
                {
                    ConcurrencyStamp = ppVisa.ConcurrencyStamp,
                    Id = ppVisa.Id,
                    Level = ppVisa.Level,
                    Name = ppVisa.Name
                };
            }
        }
    }
}