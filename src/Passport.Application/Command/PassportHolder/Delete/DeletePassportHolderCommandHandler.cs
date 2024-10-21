using Mediator;
using Passport.Abstraction.Result;
using Passport.Application.Default;
using Passport.Application.Interface;
using Passport.Application.Result;
using Passport.Application.Transfer;

namespace Passport.Application.Command.PassportHolder.Delete
{
    public sealed class DeletePassportHolderCommandHandler : ICommandHandler<DeletePassportHolderCommand, IMessageResult<bool>>
    {
        private readonly TimeProvider prvTime;
        private readonly IPassportHolderRepository repoHolder;

        public DeletePassportHolderCommandHandler(
            TimeProvider prvTime,
            IPassportHolderRepository repoHolder)
        {
            this.prvTime = prvTime;
            this.repoHolder = repoHolder;
        }

        public async ValueTask<IMessageResult<bool>> Handle(DeletePassportHolderCommand msgMessage, CancellationToken tknCancellation)
        {
            if (tknCancellation.IsCancellationRequested)
                return new MessageResult<bool>(DefaultMessageError.TaskAborted);

            RepositoryResult<PassportHolderTransferObject> rsltPassport = await repoHolder.FindByIdAsync(msgMessage.PassportHolderId, tknCancellation);

            return await rsltPassport.MatchAsync(
                msgError => new MessageResult<bool>(new MessageError() { Code = msgError.Code, Description = msgError.Description }),
                async dtoPassportHolder =>
                {
                    RepositoryResult<bool> rsltDelete = await repoHolder.DeleteAsync(dtoPassportHolder, tknCancellation);

                    return rsltDelete.Match(
                        msgError => new MessageResult<bool>(new MessageError() { Code = msgError.Code, Description = msgError.Description }),
                        bResult => new MessageResult<bool>(bResult));
                });
        }
    }
}
