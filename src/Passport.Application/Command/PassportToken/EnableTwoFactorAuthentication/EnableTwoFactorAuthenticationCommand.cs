using Mediator;
using Passport.Abstraction.Authentication;
using Passport.Abstraction.Authorization;
using Passport.Abstraction.Result;
using System;

namespace Passport.Application.Command.PassportToken.EnableTwoFactorAuthentication
{
    public sealed class EnableTwoFactorAuthenticationCommand : ICommand<IMessageResult<bool>>, IRestrictedAuthorization, IVerifiedAuthorization
    {
        public required bool TwoFactorIsEnabled { get; init; }

        public required IPassportCredential CredentialToVerify { get; init; }
        public required Guid RestrictedPassportId { get; init; }
    }
}