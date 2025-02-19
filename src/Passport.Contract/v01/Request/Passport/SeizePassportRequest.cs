using System;

namespace Passport.Contract.v01.Request.Passport
{
    public sealed class SeizePassportRequest : VerifiedRequest
    {
        public required Guid PassportId { get; init; }
    }
}