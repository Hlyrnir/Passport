using System;

namespace Passport.Contract.v01.Request.PassportHolder
{
    public sealed class ConfirmPhoneNumberRequest
    {
        public required Guid PassportHolderId { get; init; }
        public required string ConcurrencyStamp { get; init; }
        public required string PhoneNumber { get; init; }
    }
}
