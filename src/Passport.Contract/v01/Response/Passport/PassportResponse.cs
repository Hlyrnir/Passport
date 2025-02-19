using System;
using System.Collections.Generic;

namespace Passport.Contract.v01.Response.Passport
{
    public sealed class PassportResponse
    {
        public required string ConcurrencyStamp { get; init; }
        public required DateTimeOffset ExpiredAt { get; init; }
        public required Guid HolderId { get; init; }
        public required Guid Id { get; init; }
        public required bool IsAuthority { get; init; }
        public required bool IsEnabled { get; init; }
        public required Guid IssuedBy { get; init; }
        public required DateTimeOffset LastCheckedAt { get; init; }
        public required Guid LastCheckedBy { get; init; }
        public required IEnumerable<Guid> VisaId { get; init; }
    }
}
