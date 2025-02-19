using System;

namespace Passport.Contract.v01.Request.PassportHolder
{
    public sealed class DeletePassportHolderRequest : VerifiedRequest
    {
        public required Guid PassportHolderId { get; init; }
    }
}