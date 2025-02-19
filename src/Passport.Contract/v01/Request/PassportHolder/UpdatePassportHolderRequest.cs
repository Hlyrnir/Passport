using System;

namespace Passport.Contract.v01.Request.PassportHolder
{
    public sealed class UpdatePassportHolderRequest
    {
        public required Guid PassportHolderId { get; init; }
        public required string ConcurrencyStamp { get; init; }
        public required string CultureName { get; init; }
        public required string EmailAddress { get; init; }
        public required string FirstName { get; init; }
        public required string LastName { get; init; }
        public required string PhoneNumber { get; init; }
    }
}
