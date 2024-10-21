using Mediator;
using Passport.Abstraction.Authorization;
using Passport.Application.Result;

namespace Passport.Application.Query.Passport.ById
{
    public sealed class PassportByIdQuery : IQuery<MessageResult<PassportByIdResult>>, IRestrictedAuthorization
    {
        public required Guid RestrictedPassportId { get; init; }
        public required Guid PassportId { get; init; }
    }
}
