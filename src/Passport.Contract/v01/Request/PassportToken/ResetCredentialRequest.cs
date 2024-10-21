namespace Passport.Contract.v01.Request.PassportToken
{
    public sealed class ResetCredentialRequest : VerifiedRequest
    {
        public required string ProviderToApply { get; init; }
        public required string CredentialToApply { get; init; }
        public required string SignatureToApply { get; init; }
    }
}
