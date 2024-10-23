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

        public static void AddPassportEndpoint(this IEndpointRouteBuilder epBuilder, params string[] sPolicyName)
        {
            epBuilder.AddFindPassportByIdEndpoint(sPolicyName: sPolicyName);
            epBuilder.AddRegisterPassportEndpoint(sPolicyName: sPolicyName);
            epBuilder.AddSeizePassportEndpoint(sPolicyName: sPolicyName);
            epBuilder.AddUpdatePassportEndpoint(sPolicyName: sPolicyName);
        }

        public static void AddPassportHolderEndpoint(this IEndpointRouteBuilder epBuilder, params string[] sPolicyName)
        {
            epBuilder.AddConfirmEmailAddressEndpoint(sPolicyName: sPolicyName);
            epBuilder.AddConfirmPhoneNumberEndpoint(sPolicyName: sPolicyName);
            epBuilder.AddDeletePassportHolderEndpoint(sPolicyName: sPolicyName);
            epBuilder.AddFindPassportHolderByIdEndpoint(sPolicyName: sPolicyName);
            epBuilder.AddUpdatePassportHolderEndpoint(sPolicyName: sPolicyName);
        }

        public static void AddPassportTokenEndpoint(this IEndpointRouteBuilder epBuilder, params string[] sPolicyName)
        {
            epBuilder.AddCreatePassportTokenEndpoint(sPolicyName: sPolicyName);
            epBuilder.AddDeletePassportTokenEndpoint(sPolicyName: sPolicyName);
            epBuilder.AddEnableTwoFactorAuthenticationEndpoint(sPolicyName: sPolicyName);
            epBuilder.AddResetCredentialEndpoint(sPolicyName: sPolicyName);
        }

        public static void AddPassportVisaEndpoint(this IEndpointRouteBuilder epBuilder, params string[] sPolicyName)
        {
            epBuilder.AddCreatePassportVisaEndpoint(sPolicyName: sPolicyName);
            epBuilder.AddDeletePassportVisaEndpoint(sPolicyName: sPolicyName);
            epBuilder.AddFindPassportVisaByIdEndpoint(sPolicyName: sPolicyName);
            epBuilder.AddFindPassportVisaByPassportIdEndpoint(sPolicyName: sPolicyName);
            epBuilder.AddUpdatePassportVisaEndpoint(sPolicyName: sPolicyName);
        }
    }
}