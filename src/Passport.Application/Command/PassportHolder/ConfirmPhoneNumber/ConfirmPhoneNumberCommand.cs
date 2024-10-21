using Mediator;
using Passport.Abstraction.Authorization;
using Passport.Abstraction.Result;

namespace Passport.Application.Command.PassportHolder.ConfirmPhoneNumber
{
    public sealed class ConfirmPhoneNumberCommand : ICommand<IMessageResult<bool>>, IRestrictedAuthorization
    {
        public required Guid RestrictedPassportId { get; init; }

        public required string ConcurrencyStamp { get; init; }
        public required Guid PassportHolderId { get; init; }
        public required string PhoneNumber { get; init; }
    }
}
