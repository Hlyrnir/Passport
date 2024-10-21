namespace Passport.Domain
{
    public interface IPassportSetting
    {
        int MaximalAllowedAccessAttempt { get; init; }
        int MaximalCredentialLength { get; init; }
        int MaximalSignatureLength { get; init; }
        TimeSpan MaximalRefreshTokenEffectivity { get; init; }
        TimeSpan MinimalDelayBetweenAttempt { get; init; }
        TimeSpan MinimalLockoutDuration { get; init; }
        int MinimalPhoneNumberLength { get; init; }
        TimeSpan PassportExpiresAfterDuration { get; init; }
        TimeSpan RefreshTokenExpiresAfterDuration { get; init; }
        int RequiredMinimalCredentialLength { get; init; }
        int RequiredMinimalSignatureLength { get; init; }
        string RequiredDigit { get; }
        string RequiredLowerCase { get; }
        string RequiredUpperCase { get; }
        string RequiredSpecial { get; init; }
        bool TwoFactorAuthentication { get; init; }
        IEnumerable<string> ValidProviderName { get; init; }
    }
}
