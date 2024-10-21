using Passport.Application.Transfer;

namespace Passport.Application.Query.PassportVisa.ByPassportId
{
    public sealed class PassportVisaByPassportIdResult
    {
        public required IEnumerable<PassportVisaTransferObject> PassportVisa { get; init; }
    }
}
