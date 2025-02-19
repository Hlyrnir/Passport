using System;
using System.Threading.Tasks;

namespace Passport.Abstraction.Result
{
    public interface IMessageResult<T>
    {
        bool IsFailed { get; }
        bool IsSuccess { get; }

        R Match<R>(Func<IMessageError, R> MethodIfIsFailed, Func<T, R> MethodIfIsSuccess);
        Task<R> MatchAsync<R>(Func<IMessageError, R> MethodIfIsFailed, Func<T, Task<R>> MethodIfIsSuccess);
    }
}