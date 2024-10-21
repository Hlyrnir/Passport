using Passport.Abstraction.Authentication;

namespace Passport.Api
{
    internal class DefaultAuthenticationTokenHandler<T> : IAuthenticationTokenHandler<T>
    {
        public string Generate(T gId, TimeProvider prvTime)
        {
            return "AUTHENTICATION_DEFAULT_TOKEN";
        }
    }
}