using System;

namespace Passport.Contract.v01.Request.Authentication
{
    public sealed class GenerateAuthenticationTokenByRefreshTokenRequest
    {
        public required Guid PassportId { get; init; }
        public required string Provider { get; init; }
        public required string RefreshToken { get; init; }
    }
}
