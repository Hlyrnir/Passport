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
        public static void AddAuthenticationEndpoint(this IEndpointRouteBuilder epBuilder, string sCorsPolicyName, params string[] sAuthorizationPolicyName)
        {
            epBuilder.AddGenerateAuthenticationTokenByCredentialEndpoint(sCorsPolicyName: sCorsPolicyName);
            epBuilder.AddGenerateAuthenticationTokenByRefreshTokenEndpoint(sCorsPolicyName: sCorsPolicyName);
            epBuilder.AddResetRefreshTokenByPassportIdEndpoint(sCorsPolicyName: sCorsPolicyName, sAuthorizationPolicyName: sAuthorizationPolicyName);
        }

        public static void AddPassportEndpoint(this IEndpointRouteBuilder epBuilder, string sCorsPolicyName, params string[] sAuthorizationPolicyName)
        {
            epBuilder.AddFindPassportByIdEndpoint(sCorsPolicyName: sCorsPolicyName, sAuthorizationPolicyName: sAuthorizationPolicyName);
            epBuilder.AddRegisterPassportEndpoint(sCorsPolicyName: sCorsPolicyName, sAuthorizationPolicyName: sAuthorizationPolicyName);
            epBuilder.AddSeizePassportEndpoint(sCorsPolicyName: sCorsPolicyName, sAuthorizationPolicyName: sAuthorizationPolicyName);
            epBuilder.AddUpdatePassportEndpoint(sCorsPolicyName: sCorsPolicyName, sAuthorizationPolicyName: sAuthorizationPolicyName);
        }

        public static void AddPassportHolderEndpoint(this IEndpointRouteBuilder epBuilder, string sCorsPolicyName, params string[] sAuthorizationPolicyName)
        {
            epBuilder.AddConfirmEmailAddressEndpoint(sCorsPolicyName: sCorsPolicyName, sAuthorizationPolicyName: sAuthorizationPolicyName);
            epBuilder.AddConfirmPhoneNumberEndpoint(sCorsPolicyName: sCorsPolicyName, sAuthorizationPolicyName: sAuthorizationPolicyName);
            epBuilder.AddDeletePassportHolderEndpoint(sCorsPolicyName: sCorsPolicyName, sAuthorizationPolicyName: sAuthorizationPolicyName);
            epBuilder.AddFindPassportHolderByIdEndpoint(sCorsPolicyName: sCorsPolicyName, sAuthorizationPolicyName: sAuthorizationPolicyName);
            epBuilder.AddUpdatePassportHolderEndpoint(sCorsPolicyName: sCorsPolicyName, sAuthorizationPolicyName: sAuthorizationPolicyName);
        }

        public static void AddPassportTokenEndpoint(this IEndpointRouteBuilder epBuilder, string sCorsPolicyName, params string[] sAuthorizationPolicyName)
        {
            epBuilder.AddCreatePassportTokenEndpoint(sCorsPolicyName: sCorsPolicyName, sAuthorizationPolicyName: sAuthorizationPolicyName);
            epBuilder.AddDeletePassportTokenEndpoint(sCorsPolicyName: sCorsPolicyName, sAuthorizationPolicyName: sAuthorizationPolicyName);
            epBuilder.AddEnableTwoFactorAuthenticationEndpoint(sCorsPolicyName: sCorsPolicyName, sAuthorizationPolicyName: sAuthorizationPolicyName);
            epBuilder.AddResetCredentialEndpoint(sCorsPolicyName: sCorsPolicyName, sAuthorizationPolicyName: sAuthorizationPolicyName);
        }

        public static void AddPassportVisaEndpoint(this IEndpointRouteBuilder epBuilder, string sCorsPolicyName, params string[] sAuthorizationPolicyName)
        {
            epBuilder.AddCreatePassportVisaEndpoint(sCorsPolicyName: sCorsPolicyName, sAuthorizationPolicyName: sAuthorizationPolicyName);
            epBuilder.AddDeletePassportVisaEndpoint(sCorsPolicyName: sCorsPolicyName, sAuthorizationPolicyName: sAuthorizationPolicyName);
            epBuilder.AddFindPassportVisaByIdEndpoint(sCorsPolicyName: sCorsPolicyName, sAuthorizationPolicyName: sAuthorizationPolicyName);
            epBuilder.AddFindPassportVisaByPassportIdEndpoint(sCorsPolicyName: sCorsPolicyName, sAuthorizationPolicyName: sAuthorizationPolicyName);
            epBuilder.AddUpdatePassportVisaEndpoint(sCorsPolicyName: sCorsPolicyName, sAuthorizationPolicyName: sAuthorizationPolicyName);
        }
    }
}