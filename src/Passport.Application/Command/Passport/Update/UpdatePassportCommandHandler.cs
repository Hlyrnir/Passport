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

                    IDictionary<Guid, PassportVisaState> dictPassportVisaId = new Dictionary<Guid, PassportVisaState>();

                    foreach (Guid guPassportVisaId in ppPassport.VisaId)
                    {
                        dictPassportVisaId.TryAdd(guPassportVisaId, PassportVisaState.PASSPORT_VISA_REMOVE);
                    }

                    foreach (Guid guPassportVisaId in msgMessage.PassportVisaId)
                    {
                        if (dictPassportVisaId.TryAdd(guPassportVisaId, PassportVisaState.PASSPORT_VISA_ADD) == false)
                            dictPassportVisaId[guPassportVisaId] = PassportVisaState.PASSPORT_VISA_SKIP;
                    }

                    foreach (KeyValuePair<Guid, PassportVisaState> kvpPassportVisaId in dictPassportVisaId)
                    {
                        RepositoryResult<PassportVisaTransferObject> rsltVisa = await repoVisa.FindByIdAsync(kvpPassportVisaId.Key, tknCancellation);

                        MessageResult<bool> rsltVisaIsUpdated = rsltVisa.Match(
                            msgError => new MessageResult<bool>(new MessageError() { Code = msgError.Code, Description = msgError.Description }),
                            dtoPassportVisa =>
                            {
                                Domain.Aggregate.PassportVisa? ppVisa = dtoPassportVisa.Initialize();

                                if (ppVisa is null)
                                    return new MessageResult<bool>(DomainError.InitializationHasFailed);

                                switch (kvpPassportVisaId.Value)
                                {
                                    case PassportVisaState.PASSPORT_VISA_SKIP:
                                        return true;

                                    case PassportVisaState.PASSPORT_VISA_REMOVE:

                                        if (ppPassport.TryRemoveVisa(ppVisa) == false)
                                            return new MessageResult<bool>(new MessageError() { Code = DomainError.Code.Method, Description = $"Visa {ppVisa.Id} could not be removed from passport {ppPassport.Id}" });

                                        break;
                                    case PassportVisaState.PASSPORT_VISA_ADD:

                                        if (ppPassport.TryAddVisa(ppVisa) == false)
                                            return new MessageResult<bool>(new MessageError() { Code = DomainError.Code.Method, Description = $"Visa {ppVisa.Id} could not be added to passport {ppPassport.Id}" });

                                        break;
                                    default:
                                        return new MessageResult<bool>(new MessageError() { Code = DomainError.Code.Method, Description = $"Passport {ppPassport.Id} could not be updated." });
                                }

                                return true;
                            });

                        if (rsltVisaIsUpdated.IsFailed)
                            return rsltVisaIsUpdated;
                    }

                    if (ppPassport.ExpiredAt != msgMessage.ExpiredAt)
                    {
                        if (ppPassport.TryExtendTerm(msgMessage.ExpiredAt, prvTime.GetUtcNow(), msgMessage.RestrictedPassportId) == false)
                            return new MessageResult<bool>(new MessageError() { Code = DomainError.Code.Method, Description = $"Term of passport {ppPassport.Id} could not be extended." });
                    }

                    if (ppPassport.IsEnabled != msgMessage.IsEnabled || ppPassport.IsAuthority != msgMessage.IsAuthority)
                    {
                        RepositoryResult<PassportTransferObject> rsltAuthority = await repoPassport.FindByIdAsync(msgMessage.RestrictedPassportId, tknCancellation);

                        MessageResult<bool> rsltPassportIsUpdated = rsltAuthority.Match(
                            msgError => new MessageResult<bool>(new MessageError() { Code = msgError.Code, Description = msgError.Description }),
                            dtoAuthority =>
                            {
                                Domain.Aggregate.Passport? ppAuthority = dtoAuthority.Initialize();

                                if (ppAuthority is null)
                                    return new MessageResult<bool>(DomainError.InitializationHasFailed);

                                if (ppPassport.IsEnabled == false && msgMessage.IsEnabled == true)
                                {
                                    if (ppPassport.TryEnable(ppAuthority, prvTime.GetUtcNow()) == false)
                                        return new MessageResult<bool>(new MessageError() { Code = DomainError.Code.Method, Description = $"Passport {ppPassport.Id} is not enabled." });
                                }

                                if (ppPassport.IsEnabled == true && msgMessage.IsEnabled == false)
                                {
                                    if (ppPassport.TryDisable(ppAuthority, prvTime.GetUtcNow()) == false)
                                        return new MessageResult<bool>(new MessageError() { Code = DomainError.Code.Method, Description = $"Passport {ppPassport.Id} is not disabled." });
                                }

                                if (ppPassport.IsAuthority == false && msgMessage.IsAuthority == true)
                                {
                                    if (ppPassport.TryJoinToAuthority(ppAuthority, prvTime.GetUtcNow()) == false)
                                        return new MessageResult<bool>(new MessageError() { Code = DomainError.Code.Method, Description = $"Passport {ppPassport.Id} could not join to authority." });
                                }

                                if (ppPassport.IsAuthority == true && msgMessage.IsAuthority == false)
                                {
                                    if (ppPassport.TryReset(ppAuthority, prvTime.GetUtcNow()) == false)
                                        return new MessageResult<bool>(new MessageError() { Code = DomainError.Code.Method, Description = $"Passport {ppPassport.Id} could not be reset." });
                                }

                                return true;
                            });

                        if (rsltPassportIsUpdated.IsFailed)
                            return rsltPassportIsUpdated;
                    }

                    RepositoryResult<bool> rsltUpdate = await repoPassport.UpdateAsync(ppPassport.MapToTransferObject(), prvTime.GetUtcNow(), tknCancellation);

                    return rsltUpdate.Match(
                        msgError => new MessageResult<bool>(new MessageError() { Code = msgError.Code, Description = msgError.Description }),
                        bResult => new MessageResult<bool>(bResult));
                });
        }

        private enum PassportVisaState
        {
            PASSPORT_VISA_SKIP = 0,
            PASSPORT_VISA_REMOVE = 1,
            PASSPORT_VISA_ADD = 2
        }
    }
}