using Mediator;
using Passport.Abstraction.Authentication;
using Passport.Abstraction.Result;
using Passport.Application.Transfer;

namespace Passport.Application.Command.Authentication.ByCredential
{
    public sealed class AuthenticationTokenByCredentialCommand : ICommand<IMessageResult<AuthenticationTokenTransferObject>>
    {
        public required IPassportCredential Credential { get; init; }
    }
}
