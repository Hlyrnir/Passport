using System;

namespace Passport.Application.Transfer
{
    public sealed class AuthenticationTokenTransferObject
    {
        public required DateTimeOffset ExpiredAt { get; init; }
        public required string Provider { get; init; }
        public required string RefreshToken { get; init; }
        public required string Token { get; init; }
    }
}
