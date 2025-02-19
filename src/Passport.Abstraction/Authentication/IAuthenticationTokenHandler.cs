using System;

namespace Passport.Abstraction.Authentication
{
    public interface IAuthenticationTokenHandler<T>
    {
        string Generate(T gId, TimeProvider prvTime);
    }
}
