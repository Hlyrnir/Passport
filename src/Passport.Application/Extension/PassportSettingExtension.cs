using Passport.Application.Interface;
using Passport.Domain.ValueObject;

namespace Passport.Application.Extension
{
    internal static class PassportSettingExtension
    {
        public static PassportHolderSetting MapToPassportHolderSetting(this IPassportSetting ppSetting)
        {
            return new PassportHolderSetting()
            {
                MinimalPhoneNumberLength = ppSetting.MinimalPhoneNumberLength
            };
        }
    }
}