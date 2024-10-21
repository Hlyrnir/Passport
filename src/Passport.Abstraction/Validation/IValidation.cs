using Mediator;
using Passport.Abstraction.Result;

namespace Passport.Abstraction.Validation
{
    public interface IValidation<in T> where T : IMessage
    {
        public ValueTask<IMessageResult<bool>> ValidateAsync(T msgMessage, CancellationToken tknCancellation);
    }
}