using Passport.Application.Transfer;

namespace Passport.Infrastructure.Test
{
    internal static class DataTransferObjectExtension
    {
        internal static PassportTransferObject Clone(this PassportTransferObject dtoPassport, bool bResetConcurrencyStamp = false)
        {
            string sConcurrencyStamp = dtoPassport.ConcurrencyStamp;

            if (bResetConcurrencyStamp == true)
                sConcurrencyStamp = Guid.NewGuid().ToString();

            return new PassportTransferObject()
            {
                ConcurrencyStamp = sConcurrencyStamp,
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

        internal static PassportHolderTransferObject Clone(this PassportHolderTransferObject dtoPassportHolder, bool bResetConcurrencyStamp = false)
        {
            string sConcurrencyStamp = dtoPassportHolder.ConcurrencyStamp;

            if (bResetConcurrencyStamp == true)
                sConcurrencyStamp = Guid.NewGuid().ToString();

            return new PassportHolderTransferObject()
            {
                ConcurrencyStamp = sConcurrencyStamp,
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

        internal static PassportTokenTransferObject Clone(this PassportTokenTransferObject dtoPassportToken, DateTimeOffset dtExpiredAt)
        {
            if (dtoPassportToken.ExpiredAt != dtExpiredAt)
                return new PassportTokenTransferObject()
                {
                    ExpiredAt = dtExpiredAt,
                    Id = dtoPassportToken.Id,
                    PassportId = dtoPassportToken.PassportId,
                    Provider = dtoPassportToken.Provider,
                    RefreshToken = Guid.NewGuid().ToString(),
                    TwoFactorIsEnabled = dtoPassportToken.TwoFactorIsEnabled
                };

            return new PassportTokenTransferObject()
            {
                ExpiredAt = dtExpiredAt,
                Id = dtoPassportToken.Id,
                PassportId = dtoPassportToken.PassportId,
                Provider = dtoPassportToken.Provider,
                RefreshToken = dtoPassportToken.RefreshToken,
                TwoFactorIsEnabled = dtoPassportToken.TwoFactorIsEnabled
            };
        }

        internal static PassportVisaTransferObject Clone(this PassportVisaTransferObject dtoPassportVisa, bool bResetConcurrencyStamp = false)
        {
            string sConcurrencyStamp = dtoPassportVisa.ConcurrencyStamp;

            if (bResetConcurrencyStamp == true)
                sConcurrencyStamp = Guid.NewGuid().ToString();

            return new PassportVisaTransferObject()
            {
                ConcurrencyStamp = sConcurrencyStamp,
                Id = dtoPassportVisa.Id,
                Level = dtoPassportVisa.Level,
                Name = dtoPassportVisa.Name
            };
        }
    }
}