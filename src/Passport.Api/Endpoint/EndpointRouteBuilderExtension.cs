using Microsoft.AspNetCore.Routing;
using Passport.Api.Endpoint.Authentication;
using Passport.Api.Endpoint.Passport;
using Passport.Api.Endpoint.PassportHolder;
using Passport.Api.Endpoint.PassportToken;
using Passport.Api.Endpoint.PassportVisa;

namespace Passport.Api.Endpoint
{
    public static class EndpointRouteBuilderExtension
    {
        public static void AddAuthenticationEndpoint(this IEndpointRouteBuilder epBuilder)
        {
            epBuilder.AddGenerateAuthenticationTokenByCredentialEndpoint();
            epBuilder.AddGenerateAuthenticationTokenByRefreshTokenEndpoint();
        }

        public static void AddPassportEndpoint(this IEndpointRouteBuilder epBuilder)
        {
            epBuilder.AddFindPassportByIdEndpoint();
            epBuilder.AddRegisterPassportEndpoint();
            epBuilder.AddSeizePassportEndpoint();
            epBuilder.AddUpdatePassportEndpoint();
        }

        public static void AddPassportHolderEndpoint(this IEndpointRouteBuilder epBuilder)
        {
            epBuilder.AddConfirmEmailAddressEndpoint();
            epBuilder.AddConfirmPhoneNumberEndpoint();
            epBuilder.AddDeletePassportHolderEndpoint();
            epBuilder.AddFindPassportHolderByIdEndpoint();
            epBuilder.AddUpdatePassportHolderEndpoint();
        }

        public static void AddPassportTokenEndpoint(this IEndpointRouteBuilder epBuilder)
        {
            epBuilder.AddCreatePassportTokenEndpoint();
            epBuilder.AddDeletePassportTokenEndpoint();
            epBuilder.AddEnableTwoFactorAuthenticationEndpoint();
            epBuilder.AddResetCredentialEndpoint();
        }

        public static void AddPassportVisaEndpoint(this IEndpointRouteBuilder epBuilder)
        {
            epBuilder.AddCreatePassportVisaEndpoint();
            epBuilder.AddDeletePassportVisaEndpoint();
            epBuilder.AddFindPassportVisaByIdEndpoint();
            epBuilder.AddFindPassportVisaByPassportIdEndpoint();
            epBuilder.AddUpdatePassportVisaEndpoint();
        }
    }
}