using Mediator;
using Passport.Abstraction.Authentication;
using Passport.Abstraction.Authorization;
using Passport.Abstraction.Result;
using System;

namespace Passport.Application.Command.Passport.Seize
{
    public sealed class SeizePassportCommand : ICommand<IMessageResult<bool>>, IRestrictedAuthorization, IVerifiedAuthorization
    {
        public required Guid PassportIdToSeize { get; init; }

        public required IPassportCredential CredentialToVerify { get; init; }
        public required Guid RestrictedPassportId { get; init; }

    }
}
