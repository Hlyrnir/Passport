using Passport.Application.Transfer;
using Passport.Domain.Aggregate;

namespace Passport.Application.Extension
{
    internal static class PassportTokenExtension
    {
        internal static PassportToken? Initialize(this PassportTokenTransferObject dtoPassportToken)
        {
            return PassportToken.Initialize(
                dtExpiredAt: dtoPassportToken.ExpiredAt,
                guId: dtoPassportToken.Id,
                guPassportId: dtoPassportToken.PassportId,
                sProvider: dtoPassportToken.Provider,
                sRefreshToken: dtoPassportToken.RefreshToken,
                bTwoFactorIsEnabled: dtoPassportToken.TwoFactorIsEnabled);
        }

        internal static PassportTokenTransferObject MapToTransferObject(this PassportToken ppToken)
        {
            return new PassportTokenTransferObject()
            {
                ExpiredAt = ppToken.ExpiredAt,
                Id = ppToken.Id,
                PassportId = ppToken.PassportId,
                Provider = ppToken.Provider,
                RefreshToken = ppToken.RefreshToken,
                TwoFactorIsEnabled = ppToken.TwoFactorIsEnabled
            };
        }
    }
}
