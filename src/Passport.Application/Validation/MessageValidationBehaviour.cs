using Mediator;
using Passport.Abstraction.Result;
using Passport.Abstraction.Validation;
using Passport.Application.Result;

namespace Passport.Application.Validation
{
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