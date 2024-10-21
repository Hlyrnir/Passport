namespace Passport.Contract.v01.Request.PassportVisa
{
    public sealed class DeletePassportVisaRequest : VerifiedRequest
    {
        public required Guid PassportVisaId { get; init; }
    }
}