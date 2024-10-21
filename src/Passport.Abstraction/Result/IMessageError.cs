namespace Passport.Abstraction.Result
{
    public interface IMessageError
    {
        string Code { get; init; }
        string Description { get; init; }
    }
}