namespace Passport.Contract.v01.Request.Passport
{
    public sealed class UpdatePassportRequest : VerifiedRequest
    {
        public required Guid PassportId { get; init; }
        public required string ConcurrencyStamp { get; init; }
        public required DateTimeOffset ExpiredAt { get; init; }
        public required bool IsEnabled { get; init; }
        public required bool IsAuthority { get; init; }
        public required DateTimeOffset LastCheckedAt { get; init; }
        public required Guid LastCheckedBy { get; init; }
        public required IEnumerable<Guid> PassportVisa { get; init; }
    }
}