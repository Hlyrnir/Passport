using System;

namespace Passport.Contract.v01.Request.Authentication
{
    public sealed class ResetRefreshTokenByPassportIdRequest
    {
        public required Guid PassportId { get; init; }
        public required string Provider { get; init; }
    }
}
