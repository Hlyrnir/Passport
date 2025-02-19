using Mediator;
using Passport.Abstraction.Authentication;
using Passport.Abstraction.Authorization;
using Passport.Abstraction.Result;
using System;

namespace Passport.Application.Command.PassportToken.ResetCredential
{
    public sealed class ResetCredentialCommand : ICommand<IMessageResult<bool>>, IRestrictedAuthorization, IVerifiedAuthorization
    {
        public required IPassportCredential CredentialToApply { get; init; }

        public required IPassportCredential CredentialToVerify { get; init; }
        public required Guid RestrictedPassportId { get; init; }

    }
}
