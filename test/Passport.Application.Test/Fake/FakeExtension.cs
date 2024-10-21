using Passport.Application.Transfer;

namespace Passport.Application.Test.Fake
{
    internal static class FakeExtension
    {
        internal static PassportTransferObject Clone(this PassportTransferObject dtoPassport)
        {
            return new PassportTransferObject()
            {
                ConcurrencyStamp = dtoPassport.ConcurrencyStamp,
                ExpiredAt = dtoPassport.ExpiredAt,
                HolderId = dtoPassport.HolderId,
                Id = dtoPassport.Id,
                IsAuthority = dtoPassport.IsAuthority,
                IsEnabled = dtoPassport.IsEnabled,
                IssuedBy = dtoPassport.IssuedBy,
                VisaId = dtoPassport.VisaId,
                LastCheckedAt = dtoPassport.LastCheckedAt,
                LastCheckedBy = dtoPassport.LastCheckedBy
            };
        }

        internal static PassportHolderTransferObject Clone(this PassportHolderTransferObject dtoPassportHolder)
        {
            return new PassportHolderTransferObject()
            {
                ConcurrencyStamp = dtoPassportHolder.ConcurrencyStamp,
                CultureName = dtoPassportHolder.CultureName,
                EmailAddress = dtoPassportHolder.EmailAddress,
                EmailAddressIsConfirmed = dtoPassportHolder.EmailAddressIsConfirmed,
                FirstName = dtoPassportHolder.FirstName,
                Id = dtoPassportHolder.Id,
                LastName = dtoPassportHolder.LastName,
                PhoneNumber = dtoPassportHolder.PhoneNumber,
                PhoneNumberIsConfirmed = dtoPassportHolder.PhoneNumberIsConfirmed,
                SecurityStamp = dtoPassportHolder.SecurityStamp
            };
        }

        internal static PassportTokenTransferObject Clone(this PassportTokenTransferObject dtoPassportToken, bool bResetRefreshToken)
        {
            string sRefreshToken = dtoPassportToken.RefreshToken;

            if (bResetRefreshToken == true)
                sRefreshToken = Guid.NewGuid().ToString();

            return new PassportTokenTransferObject()
            {
                ExpiredAt = dtoPassportToken.ExpiredAt,
                Id = dtoPassportToken.Id,
                PassportId = dtoPassportToken.PassportId,
                Provider = dtoPassportToken.Provider,
                RefreshToken = sRefreshToken,
                TwoFactorIsEnabled = dtoPassportToken.TwoFactorIsEnabled
            };
        }

        internal static PassportVisaTransferObject Clone(this PassportVisaTransferObject dtoPassportVisa)
        {
            return new PassportVisaTransferObject()
            {
                ConcurrencyStamp = dtoPassportVisa.ConcurrencyStamp,
                Id = dtoPassportVisa.Id,
                Level = dtoPassportVisa.Level,
                Name = dtoPassportVisa.Name
            };
        }
    }
}