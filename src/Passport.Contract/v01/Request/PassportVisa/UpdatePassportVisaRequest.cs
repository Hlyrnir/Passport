namespace Passport.Contract.v01.Request.PassportVisa
{
    public class UpdatePassportVisaRequest : VerifiedRequest
    {
        public required Guid PassportVisaId { get; init; }
        public required string ConcurrencyStamp { get; init; }
        public required string Name { get; init; }
        public required int Level { get; init; }
    }
}