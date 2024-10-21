using Passport.Abstraction.Authentication;

namespace Passport.DataFaker
{
    public static class PassportCredential
    {
        public static readonly DateTimeOffset ExpiredAt = new DateTimeOffset(2000, 1, 31, 0, 0, 0, 0, 0, TimeSpan.Zero);

        public static IPassportCredential CreateDefault()
        {
            IPassportCredential ppCredential = new FakePassportCredential();

            ppCredential.Initialize(
                sProvider: Default.Provider,
                sCredential: $"{Guid.NewGuid()}@default.org",
                sSignature: $"!SIGNATURE{Guid.NewGuid()}");

            return ppCredential;
        }

        public static IPassportCredential Create(string sCredential, string sSignature)
        {
            IPassportCredential ppCredential = new FakePassportCredential();

            ppCredential.Initialize(
                sProvider: Default.Provider,
                sCredential: sCredential,
                sSignature: sSignature);

            return ppCredential;
        }

        public static IPassportCredential CreateAtProvider(string sProvider)
        {
            IPassportCredential ppCredential = new FakePassportCredential();

            ppCredential.Initialize(
                sProvider: sProvider,
                sCredential: $"{Guid.NewGuid()}@default.org",
                $"!SIGNATURE{Guid.NewGuid()}");

            return ppCredential;
        }
    }
}