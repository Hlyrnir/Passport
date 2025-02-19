using System;

namespace Passport.Application.Transfer
{
    public sealed class PassportVisaTransferObject
    {
        public required string ConcurrencyStamp { get; init; } = string.Empty;
        public required Guid Id { get; init; }
        public required int Level { get; init; }
        public required string Name { get; init; } = string.Empty;
    }
}
