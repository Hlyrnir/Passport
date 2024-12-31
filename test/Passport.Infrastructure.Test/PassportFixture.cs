using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Options;
using Microsoft.Extensions.Time.Testing;
using Passport.Application.Credential;
using Passport.Application.Interface;
using Passport.Infrastructure.Test.Fake;

namespace Passport.Infrastructure.Test
{
    public class PassportFixture
    {
        private readonly FakeTimeProvider prvTime;

        private readonly IConfiguration cfgConfiguration;
        private readonly IPassportSetting ppSetting;

        private readonly IUnitOfWork uowUnitOfWork;

        private readonly IPassportRepository repoPassport;
        private readonly IPassportHolderRepository repoHolder;
        private readonly IPassportTokenRepository repoToken;
        private readonly IPassportVisaRepository repoVisa;

        public PassportFixture()
        {
            prvTime = new FakeTimeProvider();
            prvTime.SetUtcNow(new DateTimeOffset(2000, 1, 1, 0, 0, 0, 0, 0, TimeSpan.Zero));

            cfgConfiguration = new ConfigurationBuilder()
                .AddInMemoryCollection(
                    new[]
                    {
                        new KeyValuePair<string, string?>("ConnectionStrings:DATABASE_TEST", "Data Source=D:\\Dateien\\Projekte\\CSharp\\Passport\\TEST_Passport.db; Mode=ReadWrite")
                    })
                .Build();

            IDataAccess sqlDataAccess = new SqliteDataAccess(cfgConfiguration, "DATABASE_TEST");

            IOptions<PassportHashSetting> ppHashSetting = Options.Create(new PassportHashSetting()
            {
                PublicKey = "THIS_IS_NOT_A_VALID_KEY"
            });

            PassportHasher ppHasher = new PassportHasher(ppHashSetting);

            ppSetting = new FakePassportSetting()
            {
                MaximalAllowedAccessAttempt = 2,
                ValidProviderName = new List<string>() { "DEFAULT_PROVIDER", "DEFAULT_UNDEFINED" },
                MaximalCredentialLength = 64,
                MaximalSignatureLength = 64
            };

            uowUnitOfWork = new Passport.Infrastructure.UnitOfWork(sqlDataAccess);

            repoPassport = new Passport.Infrastructure.Persistence.PassportRepository(sqlDataAccess);
            repoHolder = new Passport.Infrastructure.Persistence.PassportHolderRepository(sqlDataAccess, ppSetting);
            repoToken = new Passport.Infrastructure.Persistence.PassportTokenRepository(sqlDataAccess, ppSetting, ppHasher);
            repoVisa = new Passport.Infrastructure.Persistence.PassportVisaRepository(sqlDataAccess);
        }

        public TimeProvider TimeProvider { get => prvTime; }
        public IPassportSetting PassportSetting { get => ppSetting; }
        public IUnitOfWork UnitOfWork { get => uowUnitOfWork; }
        public IPassportRepository PassportRepository { get => repoPassport; }
        public IPassportHolderRepository PassportHolderRepository { get => repoHolder; }
        public IPassportTokenRepository PassportTokenRepository { get => repoToken; }
        public IPassportVisaRepository PassportVisaRepository { get => repoVisa; }
    }
}
