using Microsoft.Extensions.Time.Testing;
using Passport.Abstraction.Authentication;
using Passport.Application.Interface;
using Passport.Application.Test.Fake;
using Passport.Application.Test.Fake.Repository;
using Passport.Application.Validation;

namespace Passport.Application.Test
{
    public sealed class PassportFixture
    {
        private readonly FakeTimeProvider prvTime;

        private readonly IPassportSetting ppSetting;

        private readonly IPassportRepository repoPassport;
        private readonly IPassportHolderRepository repoHolder;
        private readonly IPassportTokenRepository repoToken;
        private readonly IPassportVisaRepository repoVisa;

        private readonly IUnitOfWork uowUnitOfWork;

        private readonly IAuthenticationTokenHandler<Guid> authHandler;

        private readonly FakeDatabase dbFake;

        public PassportFixture()
        {
            prvTime = new FakeTimeProvider();
            prvTime.SetUtcNow(new DateTimeOffset(2000, 1, 1, 0, 0, 0, 0, 0, TimeSpan.Zero));

            ppSetting = new FakePassportSetting()
            {
                MaximalAllowedAccessAttempt = 2,
                ValidProviderName = new List<string>() { "DEFAULT_PROVIDER", "DEFAULT_UNDEFINED" },
                MaximalCredentialLength = 64,
                MaximalSignatureLength = 64
            };

            uowUnitOfWork = new FakeUnitOfWork();

            dbFake = new FakeDatabase();

            repoPassport = new FakePassportRepository(dbFake);
            repoHolder = new FakePassportHolderRepository(dbFake, ppSetting);
            repoToken = new FakePassportTokenRepository(dbFake, ppSetting);
            repoVisa = new FakePassportVisaRepository(dbFake);

            authHandler = new FakeAuthenticationTokenHandler();
        }

        public TimeProvider TimeProvider { get => prvTime; }

        public IPassportRepository PassportRepository { get => repoPassport; }
        public IPassportHolderRepository PassportHolderRepository { get => repoHolder; }
        public IPassportTokenRepository PassportTokenRepository { get => repoToken; }
        public IPassportVisaRepository PassportVisaRepository { get => repoVisa; }

        public IUnitOfWork UnitOfWork { get => uowUnitOfWork; }
        public IPassportSetting PassportSetting { get => ppSetting; }
        public IPassportValidation PassportValidation { get => new PassportValidation(ppSetting); }

        public IAuthenticationTokenHandler<Guid> AuthenticationHandler { get => authHandler; }
    }
}
