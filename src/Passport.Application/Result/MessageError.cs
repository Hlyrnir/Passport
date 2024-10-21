using Passport.Abstraction.Result;

namespace Passport.Application.Result
{
    public readonly struct MessageError : IMessageError
    {
        public required string Code { get; init; }
        public required string Description { get; init; }
    }
}
