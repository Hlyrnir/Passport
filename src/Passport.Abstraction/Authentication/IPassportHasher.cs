namespace Passport.Abstraction.Authentication
{
    public interface IPassportHasher
    {
        byte[] HashSignature(string sUnprotectedSignature);
    }
}
