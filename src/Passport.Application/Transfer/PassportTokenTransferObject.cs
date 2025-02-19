using System;

namespace Passport.Application.Transfer
{
    public sealed class PassportTokenTransferObject
    {
        public required DateTimeOffset ExpiredAt { get; init; }
        public required Guid Id { get; init; }
        public required Guid PassportId { get; init; }
        public required string Provider { get; init; }
        public required string RefreshToken { get; init; }
        public required bool TwoFactorIsEnabled { get; init; }
    }
}
