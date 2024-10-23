namespace Passport.Application.Interface
{
    public interface IPassportSetting
    {
        int MaximalAllowedAccessAttempt { get; }
        int MaximalCredentialLength { get; }
        int MaximalSignatureLength { get; }
        TimeSpan MaximalRefreshTokenEffectivity { get; }
        TimeSpan MinimalDelayBetweenAttempt { get; }
        TimeSpan MinimalLockoutDuration { get; }
        int MinimalPhoneNumberLength { get; }
        TimeSpan PassportExpiresAfterDuration { get; }
        TimeSpan RefreshTokenExpiresAfterDuration { get; }
        int RequiredMinimalCredentialLength { get; }
        int RequiredMinimalSignatureLength { get; }
        string RequiredDigit { get; }
        string RequiredLowerCase { get; }
        string RequiredUpperCase { get; }
        string RequiredSpecial { get; }
        bool TwoFactorAuthentication { get; }
        IEnumerable<string> ValidProviderName { get; }
    }
}
