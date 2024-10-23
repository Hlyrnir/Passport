using Passport.Application.Interface;

namespace Passport.Application
{
    public sealed class PassportSetting : IPassportSetting
    {
        private int iRequiredMinimalCredentialLength = 4;
        private int iRequiredMinimalSignatureLength = 4;
        private int iMaximalCredentialLength = 32;
        private int iMaximalSignatureLength = 32;

        private const string sRequiredDigit = "0123456789";
        private const string sRequiredLowerCase = "abcdefghijklmnopqrstuvwxyz";
        private const string sRequiredUpperCase = "ABCDEFGHIJKLMNOPQRSTUVWXYZ";
        private string sRequiredSpecial = "!?@#$%&*";

        public int MaximalAllowedAccessAttempt { get; set; } = 2;
        public int MaximalCredentialLength { get => iMaximalCredentialLength; set => iMaximalCredentialLength = value; }
        public TimeSpan MaximalRefreshTokenEffectivity { get; set; } = new TimeSpan(0, 15, 0);
        public int MaximalSignatureLength { get => iMaximalSignatureLength; set => iMaximalSignatureLength = value; }
        public TimeSpan MinimalDelayBetweenAttempt { get; set; } = new TimeSpan(0, 0, 30);
        public TimeSpan MinimalLockoutDuration { get; set; } = new TimeSpan(0, 1, 0);
        public int MinimalPhoneNumberLength { get; set; } = 3;
        public TimeSpan PassportExpiresAfterDuration { get; set; } = new TimeSpan(30, 0, 0, 0);
        public TimeSpan RefreshTokenExpiresAfterDuration { get; set; } = new TimeSpan(30, 0, 0, 0);
        public int RequiredMinimalCredentialLength { get => iRequiredMinimalCredentialLength; set => iRequiredMinimalCredentialLength = value; }
        public int RequiredMinimalSignatureLength { get => iRequiredMinimalSignatureLength; set => iRequiredMinimalSignatureLength = value; }
        public string RequiredDigit { get => sRequiredDigit; }
        public string RequiredLowerCase { get => sRequiredLowerCase; }
        public string RequiredUpperCase { get => sRequiredUpperCase; }
        public string RequiredSpecial { get => sRequiredSpecial; set => sRequiredSpecial = value; }
        public bool TwoFactorAuthentication { get; set; } = false;
        public IEnumerable<string> ValidProviderName { get; set; } = new List<string>() { "DEFAULT_JWT" };
    }
}
