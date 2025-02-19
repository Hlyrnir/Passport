using Mediator;
using Passport.Abstraction.Result;
using Passport.Application.Transfer;
using System;

namespace Passport.Application.Command.Authentication.ByRefreshToken
{
    public sealed class AuthenticationTokenByRefreshTokenCommand : ICommand<IMessageResult<AuthenticationTokenTransferObject>>
    {
        public required Guid PassportId { get; init; }
        public required string Provider { get; init; }
        public required string RefreshToken { get; init; }
    }
}
