using Mediator;
using Passport.Abstraction.Authorization;
using Passport.Abstraction.Result;
using System;

namespace Passport.Application.Command.PassportHolder.ConfirmEmailAddress
{
    public sealed class ConfirmEmailAddressCommand : ICommand<IMessageResult<bool>>, IRestrictedAuthorization
    {
        public required Guid RestrictedPassportId { get; init; }

        public required string ConcurrencyStamp { get; init; }
        public required Guid PassportHolderId { get; init; }
        public required string EmailAddress { get; init; }
    }
}
