using Passport.Domain.ValueObject;

namespace Passport.DataFaker
{
    public static class PassportHolder
    {
        public static PassportHolderSetting Setting = new PassportHolderSetting()
        {
            MinimalPhoneNumberLength = 5
        };

        public static Domain.Aggregate.PassportHolder CreateDefault()
        {
            Domain.Aggregate.PassportHolder? ppPassport = Domain.Aggregate.PassportHolder.Create(
                sCultureName: "en-GB",
                sEmailAddress: $"{Guid.NewGuid()}@passport.org",
                sFirstName: "Jane",
                sLastName: "Doe",
                sPhoneNumber: "00000",
                ppSetting: Setting);

            if (ppPassport is null)
                throw new NullReferenceException();

            return ppPassport;
        }
    }
}