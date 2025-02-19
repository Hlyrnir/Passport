using Passport.Abstraction.Result;
using System;
using System.Threading.Tasks;

namespace Passport.Application.Result
{
    public readonly struct MessageResult<T> : IMessageResult<T>
    {
        private readonly MessageResultState enumState;
        private readonly T? gValue;

        private readonly IMessageError? msgError;

        public MessageResult(T gValue)
        {
            enumState = MessageResultState.Success;
            this.gValue = gValue;
            msgError = null;
        }

        public MessageResult(IMessageError msgError)
        {
            enumState = MessageResultState.Failure;
            gValue = default;
            this.msgError = msgError;
        }

        public static implicit operator MessageResult<T>(T gValue)
        {
            return new MessageResult<T>(gValue);
        }

        public bool IsFailed
        {
            get
            {
                if (enumState == MessageResultState.Failure)
                    return true;

                return false;
            }
        }

        public bool IsSuccess
        {
            get
            {
                if (enumState == MessageResultState.Success)
                    return true;

                return false;
            }
        }

        public R Match<R>(Func<IMessageError, R> MethodIfIsFailed, Func<T, R> MethodIfIsSuccess)
        {
            if (MethodIfIsSuccess is null || MethodIfIsFailed is null)
                throw new NotImplementedException("Match function is not defined.");

            if (gValue is null && msgError is null)
                throw new InvalidOperationException("No result was found.");

            if (enumState == MessageResultState.Success)
                return MethodIfIsSuccess(gValue!);

            return MethodIfIsFailed(msgError!);
        }

        public async Task<R> MatchAsync<R>(Func<IMessageError, R> MethodIfIsFailed, Func<T, Task<R>> MethodIfIsSuccess)
        {
            if (MethodIfIsSuccess is null || MethodIfIsFailed is null)
                throw new NotImplementedException("Match function is not defined.");

            if (gValue is null && msgError is null)
                throw new InvalidOperationException("No result was found.");

            if (enumState == MessageResultState.Success)
                return await MethodIfIsSuccess(gValue!);

            return MethodIfIsFailed(msgError!);
        }
    }

    public enum MessageResultState : byte
    {
        Failure,
        Success
    }
}
