using Mediator;
using Passport.Abstraction.Authorization;
using Passport.Application.Result;
using System;

namespace Passport.Application.Query.PassportHolder.ById
{
    public sealed class PassportHolderByIdQuery : IQuery<MessageResult<PassportHolderByIdResult>>, IRestrictedAuthorization
    {
        public required Guid RestrictedPassportId { get; init; }
        public required Guid PassportHolderId { get; init; }
    }
}
