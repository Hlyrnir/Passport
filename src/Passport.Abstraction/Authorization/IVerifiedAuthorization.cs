using Passport.Abstraction.Authentication;

namespace Passport.Abstraction.Authorization
{
    public interface IVerifiedAuthorization
    {
        public IPassportCredential CredentialToVerify { get; }
    }
}