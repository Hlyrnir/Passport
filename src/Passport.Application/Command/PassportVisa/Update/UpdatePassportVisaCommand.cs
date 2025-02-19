using Mediator;
using Passport.Abstraction.Authentication;
using Passport.Abstraction.Authorization;
using Passport.Application.Result;
using System;

namespace Passport.Application.Command.PassportVisa.Update
{
    public sealed class UpdatePassportVisaCommand : ICommand<MessageResult<bool>>, IRestrictedAuthorization, IVerifiedAuthorization
    {
        public required string ConcurrencyStamp { get; init; }
        public required Guid PassportVisaId { get; init; }
        public required string Name { get; init; }
        public required int Level { get; init; }

        public required IPassportCredential CredentialToVerify { get; init; }
        public required Guid RestrictedPassportId { get; init; }
    }
}
