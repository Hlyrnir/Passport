using System;

namespace Passport.Contract.v01.Request.PassportHolder
{
    public sealed class ConfirmEmailAddressRequest
    {
        public required Guid PassportHolderId { get; init; }
        public required string ConcurrencyStamp { get; init; }
        public required string EmailAddress { get; init; }
    }
}
