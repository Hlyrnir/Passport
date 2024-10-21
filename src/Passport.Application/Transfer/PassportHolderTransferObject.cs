namespace Passport.Application.Transfer
{
    public sealed class PassportHolderTransferObject
    {
        public required string ConcurrencyStamp { get; init; } = string.Empty;
        public required string CultureName { get; init; } = string.Empty;
        public required string EmailAddress { get; init; } = string.Empty;
        public required bool EmailAddressIsConfirmed { get; init; }
        public required string FirstName { get; init; } = string.Empty;
        public required Guid Id { get; init; }
        public required string LastName { get; init; } = string.Empty;
        public required string PhoneNumber { get; init; } = string.Empty;
        public required bool PhoneNumberIsConfirmed { get; init; }
        public required string SecurityStamp { get; init; } = string.Empty;
    }
}
