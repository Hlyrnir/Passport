using Passport.Application.Transfer;
using Passport.Domain.Aggregate;

namespace Passport.Application.Extension
{
    internal static class PassportVisaExtension
    {
        internal static PassportVisa? Initialize(this PassportVisaTransferObject dtoPassportVisa)
        {
            return PassportVisa.Initialize(
                sConcurrencyStamp: dtoPassportVisa.ConcurrencyStamp,
                guId: dtoPassportVisa.Id,
                sName: dtoPassportVisa.Name,
                iLevel: dtoPassportVisa.Level);
        }

        internal static PassportVisaTransferObject MapToTransferObject(this PassportVisa ppVisa)
        {
            return new PassportVisaTransferObject()
            {
                ConcurrencyStamp = ppVisa.ConcurrencyStamp,
                Id = ppVisa.Id,
                Name = ppVisa.Name,
                Level = ppVisa.Level
            };
        }
    }
}
