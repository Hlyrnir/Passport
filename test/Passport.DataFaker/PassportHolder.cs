using Passport.Domain;

namespace Passport.DataFaker
{
    public static class PassportHolder
    {
        public static Domain.Aggregate.PassportHolder CreateDefault(IPassportSetting ppSetting)
        {
            Domain.Aggregate.PassportHolder? ppPassport = Domain.Aggregate.PassportHolder.Create(
                sCultureName: "en-GB",
                sEmailAddress: $"{Guid.NewGuid()}@passport.org",
                sFirstName: "Jane",
                sLastName: "Doe",
                sPhoneNumber: "000",
                ppSetting: ppSetting);

            if (ppPassport is null)
                throw new NullReferenceException();

            return ppPassport;
        }
    }
}
