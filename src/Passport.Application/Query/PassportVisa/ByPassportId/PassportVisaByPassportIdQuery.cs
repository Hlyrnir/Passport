using Mediator;
using Passport.Abstraction.Authorization;
using Passport.Application.Result;
using System;

namespace Passport.Application.Query.PassportVisa.ByPassportId
{
    public sealed class PassportVisaByPassportIdQuery : IQuery<MessageResult<PassportVisaByPassportIdResult>>, IRestrictedAuthorization
    {
        public required Guid RestrictedPassportId { get; init; }

        public required Guid PassportIdToFind { get; init; }
    }
}
