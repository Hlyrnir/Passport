namespace Passport.Abstraction.Authentication
{
    public interface IAuthenticationTokenHandler<T>
    {
        public string Generate(T gId, TimeProvider prvTime);
    }
}
