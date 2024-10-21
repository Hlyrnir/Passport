using Mediator;
using Passport.Abstraction.Result;
using Passport.Application.Default;
using Passport.Application.Extension;
using Passport.Application.Interface;
using Passport.Application.Result;
using Passport.Application.Transfer;

namespace Passport.Application.Command.Passport.Update
{
    public sealed class UpdatePassportCommandHandler : ICommandHandler<UpdatePassportCommand, IMessageResult<bool>>
    {
        private readonly TimeProvider prvTime;

        private readonly IPassportRepository repoPassport;
        private readonly IPassportVisaRepository repoVisa;

        public UpdatePassportCommandHandler(
            TimeProvider prvTime,
            IPassportRepository repoPassport,
            IPassportVisaRepository repoVisa)
        {
            this.prvTime = prvTime;
            this.repoPassport = repoPassport;
            this.repoVisa = repoVisa;
        }

        public async ValueTask<IMessageResult<bool>> Handle(UpdatePassportCommand msgMessage, CancellationToken tknCancellation)
        {
            if (tknCancellation.IsCancellationRequested)
                return new MessageResult<bool>(DefaultMessageError.TaskAborted);

            RepositoryResult<PassportTransferObject> rsltPassport = await repoPassport.FindByIdAsync(msgMessage.PassportIdToUpdate, tknCancellation);

            return await rsltPassport.MatchAsync(
                msgError => new MessageResult<bool>(new MessageError() { Code = msgError.Code, Description = msgError.Description }),
                async dtoPassport =>
                {
                    if (dtoPassport.ConcurrencyStamp != msgMessage.ConcurrencyStamp)
                        return new MessageResult<bool>(DefaultMessageError.ConcurrencyViolation);

                    Domain.Aggregate.Passport? ppPassport = dtoPassport.Initialize();

                    if (ppPassport is null)
                        return new MessageResult<bool>(DomainError.InitializationHasFailed);

                    if (msgMessage.IsAuthority == true || msgMessage.IsEnabled == true)
                    {
                        RepositoryResult<PassportTransferObject> rsltAuthorizedPassport = await repoPassport.FindByIdAsync(msgMessage.RestrictedPassportId, tknCancellation);

                        MessageResult<bool> rsltPassportIsChanged = rsltAuthorizedPassport.Match(
                            msgError => new MessageResult<bool>(new MessageError() { Code = msgError.Code, Description = msgError.Description }),
                            dtoAuthority =>
                            {
                                Domain.Aggregate.Passport? ppAuthority = dtoAuthority.Initialize();

                                if (ppAuthority is null)
                                    return new MessageResult<bool>(DomainError.InitializationHasFailed);

                                if (ppPassport.IsAuthority == false && msgMessage.IsAuthority == true)
                                {
                                    if (ppPassport.TryJoinToAuthority(ppAuthority, prvTime.GetUtcNow()) == false)
                                        return new MessageResult<bool>(new MessageError() { Code = DomainError.Code.Method, Description = $"Passport {ppPassport.Id} could not join to authority." });
                                }

                                if (ppPassport.IsEnabled == false && msgMessage.IsEnabled == true)
                                {
                                    if (ppPassport.TryEnable(ppAuthority, prvTime.GetUtcNow()) == false)
                                        return new MessageResult<bool>(new MessageError() { Code = DomainError.Code.Method, Description = $"Passport {ppPassport.Id} is not enabled." });
                                }

                                return true;
                            });

                        if (rsltPassportIsChanged.IsFailed)
                            return rsltPassportIsChanged;
                    }

                    if (ppPassport.TryExtendTerm(msgMessage.ExpiredAt, prvTime.GetUtcNow(), msgMessage.RestrictedPassportId) == false)
                        return new MessageResult<bool>(new MessageError() { Code = DomainError.Code.Method, Description = $"Term of passport {ppPassport.Id} could not be extended." });

                    foreach (Guid guPassportVisa in msgMessage.PassportVisaId)
                    {
                        RepositoryResult<PassportVisaTransferObject> rsltVisa = await repoVisa.FindByIdAsync(guPassportVisa, tknCancellation);

                        MessageResult<bool> rsltVisaIsAdded = rsltVisa.Match(
                            msgError => new MessageResult<bool>(new MessageError() { Code = msgError.Code, Description = msgError.Description }),
                            dtoPassportVisa =>
                            {
                                Domain.Aggregate.PassportVisa? ppVisa = dtoPassportVisa.Initialize();

                                if (ppVisa is null)
                                    return new MessageResult<bool>(DomainError.InitializationHasFailed);

                                if (ppPassport.TryAddVisa(ppVisa) == false)
                                    return new MessageResult<bool>(new MessageError() { Code = DomainError.Code.Method, Description = $"Visa {ppVisa} could not be added to passport {ppPassport.Id}" });

                                return true;
                            });

                        if (rsltVisaIsAdded.IsFailed)
                            return rsltVisaIsAdded;
                    }

                    RepositoryResult<bool> rsltUpdate = await repoPassport.UpdateAsync(ppPassport.MapToTransferObject(), prvTime.GetUtcNow(), tknCancellation);

                    return rsltUpdate.Match(
                        msgError => new MessageResult<bool>(new MessageError() { Code = msgError.Code, Description = msgError.Description }),
                        bResult => new MessageResult<bool>(bResult));
                });
        }
    }
}