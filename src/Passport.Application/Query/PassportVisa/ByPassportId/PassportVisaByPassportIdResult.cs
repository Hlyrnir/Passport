using Passport.Application.Transfer;
using System.Collections.Generic;

namespace Passport.Application.Query.PassportVisa.ByPassportId
{
    public sealed class PassportVisaByPassportIdResult
    {
        public required IEnumerable<PassportVisaTransferObject> PassportVisa { get; init; }
    }
}
