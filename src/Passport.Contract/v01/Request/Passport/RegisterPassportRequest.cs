namespace Passport.Contract.v01.Request.Passport
{
    public sealed class RegisterPassportRequest
    {
        public required string Provider { get; init; }
        public required string CredentialToRegister { get; init; }
        public required string SignatureToRegister { get; init; }
        public required string CredentialToVerify { get; init; }
        public required string SignatureToVerify { get; init; }

        public required string CultureName { get; init; }
        public required string EmailAddress { get; init; }
        public required string FirstName { get; init; }
        public required string LastName { get; init; }
        public required string PhoneNumber { get; init; }
    }
}
