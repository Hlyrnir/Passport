using Mediator;
using Passport.Abstraction.Authorization;
using Passport.Abstraction.Result;

namespace Passport.Application.Test.Fake.Command
{
    public class FakeCommand : ICommand<IMessageResult<FakeResult>>, IRestrictedAuthorization
    {
        public string Test { get; init; } = string.Empty;

        public required Guid RestrictedPassportId { get; init; }
    }
}
