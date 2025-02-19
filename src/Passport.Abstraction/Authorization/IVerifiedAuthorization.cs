using Passport.Abstraction.Authentication;

namespace Passport.Abstraction.Authorization
{
    public interface IVerifiedAuthorization
    {
        IPassportCredential CredentialToVerify { get; }
    }
}