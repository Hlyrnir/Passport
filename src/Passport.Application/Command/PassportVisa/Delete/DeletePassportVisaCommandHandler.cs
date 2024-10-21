using Mediator;
using Passport.Application.Default;
using Passport.Application.Interface;
using Passport.Application.Result;
using Passport.Application.Transfer;

namespace Passport.Application.Command.PassportVisa.Delete
{
    public sealed class DeletePassportVisaCommandHandler : ICommandHandler<DeletePassportVisaCommand, MessageResult<bool>>
    {
        private readonly TimeProvider prvTime;
        private readonly IPassportVisaRepository repoVisa;

        public DeletePassportVisaCommandHandler(
            TimeProvider prvTime,
            IPassportVisaRepository repoVisa)
        {
            this.prvTime = prvTime;
            this.repoVisa = repoVisa;
        }

        public async ValueTask<MessageResult<bool>> Handle(DeletePassportVisaCommand msgMessage, CancellationToken tknCancellation)
        {
            if (tknCancellation.IsCancellationRequested)
                return new MessageResult<bool>(DefaultMessageError.TaskAborted);

            RepositoryResult<PassportVisaTransferObject> rsltVisa = await repoVisa.FindByIdAsync(msgMessage.PassportVisaId, tknCancellation);

            return await rsltVisa.MatchAsync(
                msgError => new MessageResult<bool>(new MessageError() { Code = msgError.Code, Description = msgError.Description }),
                async dtoPassportVisa =>
                {
                    RepositoryResult<bool> rsltDelete = await repoVisa.DeleteAsync(dtoPassportVisa, tknCancellation);

                    return rsltDelete.Match(
                        msgError => new MessageResult<bool>(new MessageError() { Code = msgError.Code, Description = msgError.Description }),
                        bResult => new MessageResult<bool>(bResult));
                });
        }
    }
}