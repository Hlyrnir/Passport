namespace Passport.Contract.v01.Request.Authentication
{
    public sealed class GenerateAuthenticationTokenByCredentialRequest
    {
        public required string Provider { get; init; }
        public required string Credential { get; init; }
        public required string Signature { get; init; }
    }
}
