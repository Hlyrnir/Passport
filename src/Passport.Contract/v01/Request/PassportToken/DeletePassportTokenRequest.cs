namespace Passport.Contract.v01.Request.PassportToken
{
    public sealed class DeletePassportTokenRequest : VerifiedRequest
    {
        public required string ProviderToRemove { get; init; }
        public required string CredentialToRemove { get; init; }
        public required string SignatureToRemove { get; init; }
    }
}
