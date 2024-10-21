using Passport.Abstraction.Authentication;

namespace Passport.Application.Test.Fake
{
    internal class FakeAuthenticationTokenHandler : IAuthenticationTokenHandler<Guid>
    {
        public string Generate(Guid gId, TimeProvider prvTime)
        {
            return $"{prvTime.GetUtcNow()} - {gId}";
        }
    }
}
