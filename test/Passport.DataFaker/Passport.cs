namespace Passport.DataFaker
{
    public static class Passport
    {
        public static readonly DateTimeOffset LastCheckedAt = new DateTimeOffset(2000, 1, 31, 0, 0, 0, 0, 0, TimeSpan.Zero);

        public static Domain.Aggregate.Passport CreateDefault()
        {
            Domain.Aggregate.Passport? ppPassport = Domain.Aggregate.Passport.Initialize(
            sConcurrencyStamp: Guid.NewGuid().ToString(),
            dtExpiredAt: LastCheckedAt.AddDays(5),
            guHolderId: Guid.NewGuid(),
            guId: Guid.NewGuid(),
            bIsAuthority: false,
            bIsEnabled: false,
            guIssuedBy: Guid.NewGuid(),
            dtLastCheckedAt: LastCheckedAt,
            guLastCheckedBy: Guid.NewGuid(),
            lstPassportVisaId: new List<Guid>());

            if (ppPassport is null)
                throw new NullReferenceException();

            return ppPassport;
        }

        public static Domain.Aggregate.Passport CreateDefault(IList<Guid> lstPassportVisaId)
        {
            Domain.Aggregate.Passport? ppPassport = Domain.Aggregate.Passport.Initialize(
            sConcurrencyStamp: Guid.NewGuid().ToString(),
            dtExpiredAt: LastCheckedAt.AddDays(30),
            guHolderId: Guid.NewGuid(),
            guId: Guid.NewGuid(),
            bIsAuthority: false,
            bIsEnabled: true,
            guIssuedBy: Guid.NewGuid(),
            dtLastCheckedAt: LastCheckedAt,
            guLastCheckedBy: Guid.NewGuid(),
            lstPassportVisaId: lstPassportVisaId);

            if (ppPassport is null)
                throw new NullReferenceException();

            return ppPassport;
        }

        public static Domain.Aggregate.Passport CreateAuthority()
        {
            Domain.Aggregate.Passport? ppPassport = Domain.Aggregate.Passport.Initialize(
            sConcurrencyStamp: Guid.NewGuid().ToString(),
            dtExpiredAt: LastCheckedAt.AddDays(30),
            guHolderId: Guid.NewGuid(),
            guId: Guid.NewGuid(),
            bIsAuthority: true,
            bIsEnabled: true,
            guIssuedBy: Guid.NewGuid(),
            dtLastCheckedAt: LastCheckedAt,
            guLastCheckedBy: Guid.NewGuid(),
            lstPassportVisaId: new List<Guid>());

            if (ppPassport is null)
                throw new NullReferenceException();

            return ppPassport;
        }

        public static Domain.Aggregate.Passport CreateExpiredAt(double dDay)
        {
            Domain.Aggregate.Passport? ppPassport = Domain.Aggregate.Passport.Initialize(
            sConcurrencyStamp: Guid.NewGuid().ToString(),
            dtExpiredAt: LastCheckedAt.AddDays(dDay),
            guHolderId: Guid.NewGuid(),
            guId: Guid.NewGuid(),
            bIsAuthority: false,
            bIsEnabled: false,
            guIssuedBy: Guid.NewGuid(),
            dtLastCheckedAt: LastCheckedAt,
            guLastCheckedBy: Guid.NewGuid(),
            lstPassportVisaId: new List<Guid>());

            if (ppPassport is null)
                throw new NullReferenceException();

            return ppPassport;
        }
    }
}
