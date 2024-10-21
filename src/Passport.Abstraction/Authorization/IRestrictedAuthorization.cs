namespace Passport.Abstraction.Authorization
{
    public interface IRestrictedAuthorization
    {
        public Guid RestrictedPassportId { get; }
    }
}
