using System;

namespace Passport.Abstraction.Authorization
{
    public interface IRestrictedAuthorization
    {
        Guid RestrictedPassportId { get; }
    }
}
