namespace Passport.DataFaker
{
    public static class PassportToken
    {
        public static readonly DateTimeOffset ExpiredAt = new DateTimeOffset(2000, 1, 31, 0, 0, 0, 0, 0, TimeSpan.Zero);

        public static Domain.Aggregate.PassportToken CreateDefault(Guid guPassportId)
        {
            Domain.Aggregate.PassportToken? ppToken = Domain.Aggregate.PassportToken.Create(
                dtExpiredAt: ExpiredAt,
                guPassportId: guPassportId,
                sProvider: Default.Provider,
                bTwoFactorIsEnabled: false);

            if (ppToken is null)
                throw new NullReferenceException();

            return ppToken;
        }
    }
}