using Mediator;
using Passport.Abstraction.Authentication;
using Passport.Abstraction.Authorization;
using Passport.Abstraction.Result;
using System;
using System.Collections.Generic;

namespace Passport.Application.Command.Passport.Update
{
    public sealed class UpdatePassportCommand : ICommand<IMessageResult<bool>>, IRestrictedAuthorization, IVerifiedAuthorization
    {
        public required string ConcurrencyStamp { get; init; }
        public required Guid PassportIdToUpdate { get; init; }
        public required DateTimeOffset ExpiredAt { get; init; }
        public required bool IsEnabled { get; init; }
        public required bool IsAuthority { get; init; }
        public required DateTimeOffset LastCheckedAt { get; init; }
        public required Guid LastCheckedBy { get; init; }
        public required IEnumerable<Guid> PassportVisaId { get; init; }

        public required IPassportCredential CredentialToVerify { get; init; }
        public required Guid RestrictedPassportId { get; init; }
    }
}
