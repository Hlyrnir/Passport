using Passport.Domain;

namespace Passport.DataFaker
{
    public static class PassportSetting
    {
        public static IPassportSetting Create()
        {
            return new FakePassportSetting()
            {
                MaximalAllowedAccessAttempt = 2,
                ValidProviderName = new List<string>() { Default.Provider, "DEFAULT_UNDEFINED" },
                MaximalCredentialLength = 64,
                MaximalSignatureLength = 64
            };
        }
    }
}
