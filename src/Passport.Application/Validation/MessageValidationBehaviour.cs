using Mediator;
using Passport.Abstraction.Result;
using Passport.Abstraction.Validation;
using Passport.Application.Result;
using System.Threading;
using System.Threading.Tasks;

namespace Passport.Application.Validation
{
    /// <summary>
    /// Important: <see cref="TResponse"/> is implemented as <see cref="IMessageResult{TResponse}"/>
    /// </summary>
    /// <typeparam name="TMessage"></typeparam>
    /// <typeparam name="TResponse">Do not use IMessageResult!</typeparam>
    internal sealed class MessageValidationBehaviour<TMessage, TResponse> : IPipelineBehavior<TMessage, IMessageResult<TResponse>>
        where TMessage : notnull, IMessage
    {
        private readonly IValidation<TMessage> msgValidation;

        public MessageValidationBehaviour(IValidation<TMessage> msgValidation)
        {
            this.msgValidation = msgValidation;
        }

        public async ValueTask<IMessageResult<TResponse>> Handle(TMessage msgMessage, CancellationToken tknCancellation, MessageHandlerDelegate<TMessage, IMessageResult<TResponse>> dlgMessageHandler)
        {
            IMessageResult<bool> msgResult = await msgValidation.ValidateAsync(msgMessage, tknCancellation);

            return await msgResult.MatchAsync(
                msgError => new MessageResult<TResponse>(new MessageError() { Code = msgError.Code, Description = msgError.Description }),
                async bResult =>
                {
                    return await dlgMessageHandler(msgMessage, tknCancellation);
                });
        }
    }
}