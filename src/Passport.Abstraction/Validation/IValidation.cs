using Mediator;
using Passport.Abstraction.Result;
using System.Threading;
using System.Threading.Tasks;

namespace Passport.Abstraction.Validation
{
    public interface IValidation<in T> where T : IMessage
    {
        ValueTask<IMessageResult<bool>> ValidateAsync(T msgMessage, CancellationToken tknCancellation);
    }
}