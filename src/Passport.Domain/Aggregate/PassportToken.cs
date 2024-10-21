namespace Passport.Domain.Aggregate
{
    public sealed class PassportToken
    {
        private DateTimeOffset dtExpiredAt;
        private Guid guId;
        private Guid guPassportId;
        private string sProvider;
        private string sRefreshToken;
        private bool bTwoFactorIsEnabled;

        private PassportToken(
            DateTimeOffset dtExpiredAt,
            Guid guId,
            Guid guPassportId,
            string sProvider,
            string sRefreshToken,
            bool bTwoFactorIsEnabled)
        {
            this.dtExpiredAt = dtExpiredAt;
            this.guId = guId;
            this.guPassportId = guPassportId;
            this.sProvider = sProvider;
            this.sRefreshToken = sRefreshToken;
            this.bTwoFactorIsEnabled = bTwoFactorIsEnabled;
        }

        public DateTimeOffset ExpiredAt { get => dtExpiredAt; }
        public Guid Id { get => guId; }
        public Guid PassportId { get => guPassportId; }
        public string Provider { get => sProvider; }
        public string RefreshToken { get => sRefreshToken; }
        public bool TwoFactorIsEnabled { get => bTwoFactorIsEnabled; }

        public bool TryEnableTwoFactorAuthentication(bool bEnable = false)
        {
            if (bTwoFactorIsEnabled == bEnable)
                return false;

            bTwoFactorIsEnabled = bEnable;

            return true;
        }

        public static PassportToken? Initialize(
            DateTimeOffset dtExpiredAt,
            Guid guId,
            Guid guPassportId,
            string sProvider,
            string sRefreshToken,
            bool bTwoFactorIsEnabled)
        {
            if (string.IsNullOrWhiteSpace(sProvider) == true)
                return null;

            return new PassportToken(
                dtExpiredAt: dtExpiredAt,
                guId: guId,
                guPassportId: guPassportId,
                sProvider: sProvider,
                sRefreshToken: sRefreshToken,
                bTwoFactorIsEnabled: bTwoFactorIsEnabled);
        }

        public static PassportToken? Create(
            DateTimeOffset dtExpiredAt,
            Guid guPassportId,
            string sProvider,
            bool bTwoFactorIsEnabled)
        {
            return Initialize(
                dtExpiredAt: dtExpiredAt,
                guId: Guid.NewGuid(),
                guPassportId: guPassportId,
                sProvider: sProvider,
                sRefreshToken: Guid.NewGuid().ToString(),
                bTwoFactorIsEnabled: bTwoFactorIsEnabled);
        }
    }
}
