namespace Passport.Infrastructure.Name
{
    internal enum PassportTokenColumn
    {
        CreatedAt,
        Credential,
        EditedAt,
        ExpiredAt,
        FailedAttemptCounter,
        Id,
        LastFailedAt,
        PassportId,
        Provider,
        RefreshToken,
        Signature,
        TwoFactorIsEnabled
    }

    internal enum PassportTokenTable
    {
        PassportToken
    }
}
