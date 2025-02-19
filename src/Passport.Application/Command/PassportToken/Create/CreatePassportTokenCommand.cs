using Mediator;
using Passport.Abstraction.Authentication;
using Passport.Abstraction.Authorization;
using Passport.Abstraction.Result;
using System;

namespace Passport.Application.Command.PassportToken.Create
{
    public sealed class CreatePassportTokenCommand : ICommand<IMessageResult<Guid>>, IRestrictedAuthorization
    {
        public required IPassportCredential CredentialToAdd { get; init; }

        public required IPassportCredential CredentialToVerify { get; init; }
        public required Guid RestrictedPassportId { get; init; }
    }
}
