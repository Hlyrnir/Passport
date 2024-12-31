using Mediator;
using Passport.Abstraction.Authorization;
using Passport.Abstraction.Result;

namespace Passport.Application.Command.Authentication.Reset
{
    public sealed class ResetRefreshTokenByPassportIdCommand : ICommand<IMessageResult<bool>>, IRestrictedAuthorization
    {
        public required Guid PassportId { get; init; }
        public required string Provider { get; init; }
        public required Guid RestrictedPassportId { get; init; }
    }
}