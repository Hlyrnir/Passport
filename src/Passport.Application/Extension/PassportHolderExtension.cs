using Passport.Application.Transfer;
using Passport.Domain;
using Passport.Domain.Aggregate;

namespace Passport.Application.Extension
{
    internal static class PassportHolderExtension
    {
        internal static PassportHolder? Initialize(this PassportHolderTransferObject dtoPassportHolder, IPassportSetting ppSetting)
        {
            return PassportHolder.Initialize(
                sConcurrencyStamp: dtoPassportHolder.ConcurrencyStamp,
                sCultureName: dtoPassportHolder.CultureName,
                sEmailAddress: dtoPassportHolder.EmailAddress,
                bEmailAddressIsConfirmed: dtoPassportHolder.EmailAddressIsConfirmed,
                sFirstName: dtoPassportHolder.FirstName,
                guId: dtoPassportHolder.Id,
                sLastName: dtoPassportHolder.LastName,
                sPhoneNumber: dtoPassportHolder.PhoneNumber,
                bPhoneNumberIsConfirmed: dtoPassportHolder.PhoneNumberIsConfirmed,
                sSecurityStamp: dtoPassportHolder.SecurityStamp,
                ppSetting: ppSetting);
        }

        internal static PassportHolderTransferObject MapToTransferObject(this PassportHolder ppHolder)
        {
            return new PassportHolderTransferObject()
            {
                ConcurrencyStamp = ppHolder.ConcurrencyStamp,
                CultureName = ppHolder.CultureName,
                EmailAddress = ppHolder.EmailAddress,
                EmailAddressIsConfirmed = ppHolder.EmailAddressIsConfirmed,
                FirstName = ppHolder.FirstName,
                Id = ppHolder.Id,
                LastName = ppHolder.LastName,
                PhoneNumber = ppHolder.PhoneNumber,
                PhoneNumberIsConfirmed = ppHolder.PhoneNumberIsConfirmed,
                SecurityStamp = ppHolder.SecurityStamp
            };
        }
    }
}
