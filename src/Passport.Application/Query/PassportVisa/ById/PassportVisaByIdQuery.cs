using Mediator;
using Passport.Abstraction.Authorization;
using Passport.Application.Result;

namespace Passport.Application.Query.PassportVisa.ById
{
    public sealed class PassportVisaByIdQuery : IQuery<MessageResult<PassportVisaByIdResult>>, IRestrictedAuthorization
    {
        public required Guid RestrictedPassportId { get; init; }
        public required Guid PassportVisaId { get; init; }
    }
}
