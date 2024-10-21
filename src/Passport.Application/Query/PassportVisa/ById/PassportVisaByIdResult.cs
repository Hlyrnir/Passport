using Passport.Application.Transfer;

namespace Passport.Application.Query.PassportVisa.ById
{
    public sealed class PassportVisaByIdResult
    {
        public required PassportVisaTransferObject PassportVisa { get; init; }
    }
}
