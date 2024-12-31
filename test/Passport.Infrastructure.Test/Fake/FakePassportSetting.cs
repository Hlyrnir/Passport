using Passport.Application.Interface;

namespace Passport.Infrastructure.Test.Fake
{
    internal class FakePassportSetting : IPassportSetting
    {
        private int iRequiredMinimalCredentialLength = 4;
        private int iRequiredMinimalSignatureLength = 4;
        private int iMaximalCredentialLength = 32;
        private int iMaximalSignatureLength = 32;

        private const string sRequiredDigit = "0123456789";
        private const string sRequiredLowerCase = "abcdefghijklmnopqrstuvwxyz";
        private const string sRequiredUpperCase = "ABCDEFGHIJKLMNOPQRSTUVWXYZ";
        private string sRequiredSpecial = "!?@#$%&*";

        public int MaximalAllowedAccessAttempt { get; init; } = 2;
        public int MaximalCredentialLength { get => iMaximalCredentialLength; init => iMaximalCredentialLength = value; }
        public int MaximalSignatureLength { get => iMaximalSignatureLength; init => iMaximalSignatureLength = value; }
        public TimeSpan MinimalDelayBetweenAttempt { get; init; } = new TimeSpan(0, 0, 30);
        public TimeSpan MinimalLockoutDuration { get; init; } = new TimeSpan(0, 1, 0);
        public int MinimalPhoneNumberLength { get; init; } = 3;
        public TimeSpan PassportExpiresAfterDuration { get; init; } = new TimeSpan(30, 0, 0, 0);
        public TimeSpan RefreshTokenExpiresAfterDuration { get; init; } = new TimeSpan(1, 0, 0, 0);
        public int RequiredMinimalCredentialLength { get => iRequiredMinimalCredentialLength; init => iRequiredMinimalCredentialLength = value; }
        public int RequiredMinimalSignatureLength { get => iRequiredMinimalSignatureLength; init => iRequiredMinimalSignatureLength = value; }
        public string RequiredDigit { get => sRequiredDigit; }
        public string RequiredLowerCase { get => sRequiredLowerCase; }
        public string RequiredUpperCase { get => sRequiredUpperCase; }
        public string RequiredSpecial { get => sRequiredSpecial; init => sRequiredSpecial = value; }
        public bool TwoFactorAuthentication { get; init; } = false;
        public IEnumerable<string> ValidProviderName { get; init; } = new List<string>();
    }
}
