using Mediator;
using Passport.Abstraction.Authentication;
using Passport.Abstraction.Authorization;
using Passport.Abstraction.Result;

namespace Passport.Application.Command.Passport.Register
{
    public sealed class RegisterPassportCommand : ICommand<IMessageResult<Guid>>, IRestrictedAuthorization
    {
        public required Guid RestrictedPassportId { get; init; }
        public required Guid IssuedBy { get; init; }

        public required IPassportCredential CredentialToRegister { get; init; }

        public required string CultureName { get; init; }
        public required string EmailAddress { get; init; }
        public required string FirstName { get; init; }
        public required string LastName { get; init; }
        public required string PhoneNumber { get; init; }
    }
}
