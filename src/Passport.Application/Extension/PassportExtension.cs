using Passport.Application.Transfer;

namespace Passport.Application.Extension
{
    internal static class PassportExtension
    {
        internal static Domain.Aggregate.Passport? Initialize(this PassportTransferObject dtoPassport)
        {
            return Domain.Aggregate.Passport.Initialize(
                sConcurrencyStamp: dtoPassport.ConcurrencyStamp,
                dtExpiredAt: dtoPassport.ExpiredAt,
                guHolderId: dtoPassport.HolderId,
                guId: dtoPassport.Id,
                bIsAuthority: dtoPassport.IsAuthority,
                bIsEnabled: dtoPassport.IsEnabled,
                guIssuedBy: dtoPassport.IssuedBy,
                dtLastCheckedAt: dtoPassport.LastCheckedAt,
                guLastCheckedBy: dtoPassport.LastCheckedBy,
                lstPassportVisaId: dtoPassport.VisaId.ToList());
        }

        internal static PassportTransferObject MapToTransferObject(this Domain.Aggregate.Passport ppPassport)
        {
            return new PassportTransferObject()
            {
                ConcurrencyStamp = ppPassport.ConcurrencyStamp,
                ExpiredAt = ppPassport.ExpiredAt,
                HolderId = ppPassport.HolderId,
                Id = ppPassport.Id,
                IsAuthority = ppPassport.IsAuthority,
                IsEnabled = ppPassport.IsEnabled,
                IssuedBy = ppPassport.IssuedBy,
                LastCheckedAt = ppPassport.LastCheckedAt,
                LastCheckedBy = ppPassport.LastCheckedBy,
                VisaId = ppPassport.VisaId
            };
        }
    }
}