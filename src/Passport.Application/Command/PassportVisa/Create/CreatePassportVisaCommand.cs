using Mediator;
using Passport.Abstraction.Authorization;
using Passport.Application.Result;
using System;

namespace Passport.Application.Command.PassportVisa.Create
{
    public sealed class CreatePassportVisaCommand : ICommand<MessageResult<Guid>>, IRestrictedAuthorization
    {
        public required Guid RestrictedPassportId { get; init; }

        public required string Name { get; init; }
        public required int Level { get; init; }
    }
}
