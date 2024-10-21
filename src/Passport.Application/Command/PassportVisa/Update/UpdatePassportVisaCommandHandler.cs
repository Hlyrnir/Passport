using Mediator;
using Passport.Application.Default;
using Passport.Application.Extension;
using Passport.Application.Interface;
using Passport.Application.Result;
using Passport.Application.Transfer;

namespace Passport.Application.Command.PassportVisa.Update
{
    public sealed class UpdatePassportVisaCommandHandler : ICommandHandler<UpdatePassportVisaCommand, MessageResult<bool>>
    {
        private readonly TimeProvider prvTime;
        private readonly IPassportVisaRepository repoVisa;

        public UpdatePassportVisaCommandHandler(
            TimeProvider prvTime,
            IPassportVisaRepository repoVisa)
        {
            this.prvTime = prvTime;
            this.repoVisa = repoVisa;
        }

        public async ValueTask<MessageResult<bool>> Handle(UpdatePassportVisaCommand msgMessage, CancellationToken tknCancellation)
        {
            if (tknCancellation.IsCancellationRequested)
                return new MessageResult<bool>(DefaultMessageError.TaskAborted);

            RepositoryResult<PassportVisaTransferObject> rsltVisa = await repoVisa.FindByIdAsync(msgMessage.PassportVisaId, tknCancellation);

            return await rsltVisa.MatchAsync(
                msgError => new MessageResult<bool>(new MessageError() { Code = msgError.Code, Description = msgError.Description }),
                async dtoPassportVisa =>
                {
                    if (dtoPassportVisa.ConcurrencyStamp != msgMessage.ConcurrencyStamp)
                        return new MessageResult<bool>(DefaultMessageError.ConcurrencyViolation);

                    Domain.Aggregate.PassportVisa? ppVisa = dtoPassportVisa.Initialize();

                    if (ppVisa is null)
                        return new MessageResult<bool>(DomainError.InitializationHasFailed);

                    if (ppVisa.Name != msgMessage.Name)
                    {
                        if (ppVisa.TryChangeName(msgMessage.Name) == false)
                            return new MessageResult<bool>(new MessageError() { Code = DomainError.Code.Method, Description = "Name could not be changed." });
                    }

                    if (ppVisa.Level != msgMessage.Level)
                    {
                        if (ppVisa.TryChangeLevel(msgMessage.Level) == false)
                            return new MessageResult<bool>(new MessageError() { Code = DomainError.Code.Method, Description = "Level could not be changed." });
                    }

                    RepositoryResult<bool> rsltUpdate = await repoVisa.UpdateAsync(ppVisa.MapToTransferObject(), prvTime.GetUtcNow(), tknCancellation);

                    return rsltUpdate.Match(
                        msgError => new MessageResult<bool>(new MessageError() { Code = msgError.Code, Description = msgError.Description }),
                        bResult => new MessageResult<bool>(bResult));
                });
        }
    }
}