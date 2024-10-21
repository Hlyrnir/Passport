namespace Passport.Contract.v01.Request.PassportToken
{
    public sealed class CreatePassportTokenRequest : VerifiedRequest
    {
        public required string ProviderToAdd { get; init; }
        public required string CredentialToAdd { get; init; }
        public required string SignatureToAdd { get; init; }
    }
}
