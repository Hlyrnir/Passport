using Mediator;
using Passport.Abstraction.Authentication;
using Passport.Abstraction.Authorization;
using Passport.Application.Result;

namespace Passport.Application.Command.PassportVisa.Delete
{
    public sealed class DeletePassportVisaCommand : ICommand<MessageResult<bool>>, IRestrictedAuthorization, IVerifiedAuthorization
    {
        public required Guid PassportVisaId { get; init; }

        public required IPassportCredential CredentialToVerify { get; init; }
        public required Guid RestrictedPassportId { get; init; }
    }
}
