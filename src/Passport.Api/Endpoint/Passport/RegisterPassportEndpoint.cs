using Mediator;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;
using Passport.Abstraction.Authentication;
using Passport.Abstraction.Result;
using Passport.Application.Command.Passport.Register;
using Passport.Application.Default;
using Passport.Contract.v01.Request.Passport;

namespace Passport.Api.Endpoint.Passport
{
    internal static class RegisterPassportEndpoint
    {
        public const string Name = "RegisterPassport";

        public static void AddRegisterPassportEndpoint(this IEndpointRouteBuilder epBuilder, params string[] sPolicyName)
        {
            epBuilder.MapPost(
                EndpointRoute.Passport.Register, RegisterPassport)
                .AllowAnonymous()
                .RequireAuthorization(sPolicyName)
                .WithName(Name)
                .WithTags("Passport")
                .Produces(StatusCodes.Status401Unauthorized)
                .Produces(StatusCodes.Status403Forbidden)
                .Produces(StatusCodes.Status201Created)
                .Produces<string>(StatusCodes.Status400BadRequest)
                .WithApiVersionSet(EndpointVersion.VersionSet!)
                .HasApiVersion(1.0);
        }

        private static async Task<IResult> RegisterPassport(
            RegisterPassportRequest rqstPassport,
            IPassportCredential ppCredentialToRegister,
            IPassportCredential ppCredentialToVerify,
            HttpContext httpContext,
            ISender mdtMediator,
            CancellationToken tknCancellation)
        {
            Guid guPassportId = Guid.Empty;

            if (httpContext.TryParsePassportId(out guPassportId) == false)
                return Results.BadRequest("Passport could not be identified.");

            RegisterPassportCommand cmdRegister = rqstPassport.MapToCommand(
                guPassportId: guPassportId,
                ppCredentialToRegister: ppCredentialToRegister,
                ppCredentialToVerify: ppCredentialToVerify);

            IMessageResult<Guid> mdtResult = await mdtMediator.Send(cmdRegister, tknCancellation);

            return mdtResult.Match(
                msgError =>
                {
                    if (msgError.Equals(AuthorizationError.PassportVisa.VisaDoesNotExist) == true)
                        return Results.Forbid();

                    return Results.BadRequest($"{msgError.Code}: {msgError.Description}");
                },
                guPassportId => TypedResults.CreatedAtRoute(FindPassportByIdEndpoint.Name, new { guPassportIdToFind = guPassportId }));
        }

        private static RegisterPassportCommand MapToCommand(this RegisterPassportRequest cmdRequest, Guid guPassportId, IPassportCredential ppCredentialToRegister, IPassportCredential ppCredentialToVerify)
        {
            ppCredentialToRegister.Initialize(
                sProvider: cmdRequest.Provider,
                sCredential: cmdRequest.CredentialToRegister,
                sSignature: cmdRequest.SignatureToRegister);

            ppCredentialToVerify.Initialize(
                sProvider: cmdRequest.Provider,
                sCredential: cmdRequest.CredentialToVerify,
                sSignature: cmdRequest.SignatureToVerify);

            return new RegisterPassportCommand()
            {
                RestrictedPassportId = guPassportId,
                IssuedBy = Guid.NewGuid(),
                CredentialToRegister = ppCredentialToRegister,
                CultureName = cmdRequest.CultureName,
                EmailAddress = cmdRequest.EmailAddress,
                FirstName = cmdRequest.FirstName,
                LastName = cmdRequest.LastName,
                PhoneNumber = cmdRequest.PhoneNumber
            };
        }
    }
}
