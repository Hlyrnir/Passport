using Mediator;
using Passport.Abstraction.Authorization;
using Passport.Abstraction.Result;
using Passport.Application.Default;
using Passport.Application.Extension;
using Passport.Application.Interface;
using Passport.Application.Result;
using Passport.Application.Transfer;

namespace Passport.Application.Authorization
{
    /// <summary>
    /// Important: <see cref="TResponse"/> is implemented as <see cref="IMessageResult{TResponse}"/>
    /// </summary>
    /// <typeparam name="TMessage"></typeparam>
    /// <typeparam name="TResponse">Do not use IMessageResult!</typeparam>
    internal sealed class MessageAuthorizationBehaviour<TMessage, TResponse> : IPipelineBehavior<TMessage, IMessageResult<TResponse>>
        where TMessage : notnull, IMessage, IRestrictedAuthorization
    {
        private readonly IAuthorization<TMessage> msgAuthorization;
        private readonly TimeProvider prvTime;

        private readonly IPassportRepository repoPassport;
        private readonly IPassportVisaRepository repoVisa;

        public MessageAuthorizationBehaviour(
            IAuthorization<TMessage> msgAuthorization,
            TimeProvider prvTime,
            IPassportRepository repoPassport,
            IPassportVisaRepository repoVisa)
        {
            this.msgAuthorization = msgAuthorization;
            this.prvTime = prvTime;
            this.repoPassport = repoPassport;
            this.repoVisa = repoVisa;
        }

        public async ValueTask<IMessageResult<TResponse>> Handle(TMessage msgMessage, CancellationToken tknCancellation, MessageHandlerDelegate<TMessage, IMessageResult<TResponse>> dlgMessageHandler)
        {
            if (tknCancellation.IsCancellationRequested)
                return new MessageResult<TResponse>(DefaultMessageError.TaskAborted);

            RepositoryResult<PassportTransferObject> rsltPassport = await repoPassport.FindByIdAsync(msgMessage.RestrictedPassportId, tknCancellation);

            return await rsltPassport.MatchAsync(
                msgError => new MessageResult<TResponse>(new MessageError() { Code = msgError.Code, Description = msgError.Description }),
                async dtoPassport =>
                {
                    Domain.Aggregate.Passport? ppPassport = dtoPassport.Initialize();

                    if (ppPassport is null)
                        return new MessageResult<TResponse>(DomainError.InitializationHasFailed);

                    if (ppPassport.IsEnabled == false)
                        return new MessageResult<TResponse>(AuthorizationError.Passport.IsDisabled);

                    if (ppPassport.IsExpired(prvTime.GetUtcNow()) == true)
                        return new MessageResult<TResponse>(AuthorizationError.Passport.IsExpired);

                    RepositoryResult<PassportVisaTransferObject> rsltVisa = await repoVisa.FindByNameAsync(msgAuthorization.PassportVisaName, msgAuthorization.PassportVisaLevel, tknCancellation);

                    return await rsltVisa.MatchAsync(
                        msgError => new MessageResult<TResponse>(new MessageError() { Code = msgError.Code, Description = msgError.Description }),
                        async dtoPassportVisa =>
                        {
                            bool bResult = false;

                            foreach (Guid guPassportVisaId in ppPassport.VisaId)
                            {
                                if (Equals(guPassportVisaId, dtoPassportVisa.Id) == true)
                                {
                                    bResult = true;
                                    break;
                                }
                            }

                            if (bResult == false)
                                return new MessageResult<TResponse>(AuthorizationError.PassportVisa.VisaDoesNotExist);

                            IMessageResult<bool> rsltAuthorization = await msgAuthorization.AuthorizeAsync(msgMessage, tknCancellation);

                            return await rsltAuthorization.MatchAsync(
                                msgError => new MessageResult<TResponse>(new MessageError() { Code = msgError.Code, Description = msgError.Description }),
                                async bResult =>
                                {
                                    return await dlgMessageHandler(msgMessage, tknCancellation);
                                });
                        });
                });

            //if (msgMessage is IVerifiedAuthorization)
            //{

            //}
        }
    }
}