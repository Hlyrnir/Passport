namespace Passport.Api.Endpoint
{
    internal static class EndpointRoute
    {
        private const string EndpointBase = "/api";

        public static class Authentication
        {
            public const string Token = $"{EndpointBase}/token";
            public const string RefreshToken = $"{EndpointBase}/refresh";
        }

        public static class Passport
        {
            public const string Base = $"{EndpointBase}/passport";
            public const string Create = Base;
            public const string Delete = Base;
            public const string GetById = $"{Base}/{{guPassportIdToFind:Guid}}";
            public const string Update = Base;

            public const string Register = $"{Base}/register";
        }

        public static class PassportHolder
        {
            private const string Base = $"{EndpointBase}/holder";
            public const string Create = Base;
            public const string Delete = Base;
            public const string GetById = $"{Base}/{{guPassportHolderIdToFind:Guid}}";
            public const string Update = Base;
            public const string ConfirmEmailAddress = $"{Base}/confirm_email";
            public const string ConfirmPhoneNumber = $"{Base}/confirm_phone";
        }

        public static class PassportToken
        {
            public const string Base = $"{EndpointBase}/token";
            public const string Create = $"{Base}/add";
            public const string Delete = $"{Base}/remove";
            public const string ResetCredential = $"{Base}/reset";
            public const string TwoFactorAuthentication = $"{Base}/tfa";
        }

        public static class PassportVisa
        {
            public const string Base = $"{EndpointBase}/visa";
            public const string Create = Base;
            public const string Delete = Base;
            public const string GetById = $"{Base}/{{guPassportVisaIdToFind:Guid}}";
            public const string GetByPassportId = $"{Base}/passport";
            public const string GetUnspecific = Base;
            public const string Update = Base;
        }
    }
}
