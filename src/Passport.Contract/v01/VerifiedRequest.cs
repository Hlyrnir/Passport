namespace Passport.Contract.v01
{
    public abstract class VerifiedRequest
    {
        public required string ProviderToVerify { get; init; }
        public required string CredentialToVerify { get; init; }
        public required string SignatureToVerify { get; init; }
    }
}
