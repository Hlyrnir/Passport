using Mediator;
using Passport.Abstraction.Result;
using Passport.Application.Default;
using Passport.Application.Interface;
using Passport.Application.Result;
using Passport.Application.Transfer;

namespace Passport.Application.Command.PassportToken.Delete
{
    public sealed class DeletePassportTokenCommandHandler : ICommandHandler<DeletePassportTokenCommand, IMessageResult<bool>>
    {
        private readonly TimeProvider prvTime;

        private readonly IPassportTokenRepository repoToken;

        public DeletePassportTokenCommandHandler(
            TimeProvider prvTime,
            IPassportTokenRepository repoToken)
        {
            this.prvTime = prvTime;
            this.repoToken = repoToken;
        }

        public async ValueTask<IMessageResult<bool>> Handle(DeletePassportTokenCommand msgMessage, CancellationToken tknCancellation)
        {
            if (tknCancellation.IsCancellationRequested)
                return new MessageResult<bool>(DefaultMessageError.TaskAborted);

            RepositoryResult<PassportTokenTransferObject> rsltToken = await repoToken.FindTokenByCredentialAsync(msgMessage.CredentialToRemove, prvTime.GetUtcNow(), tknCancellation);

            return await rsltToken.MatchAsync(
                msgError => new MessageResult<bool>(new MessageError() { Code = msgError.Code, Description = msgError.Description }),
                async dtoPassportToken =>
                {
                    RepositoryResult<bool> rsltDelete = await repoToken.DeleteAsync(dtoPassportToken, tknCancellation);

                    return rsltDelete.Match(
                        msgError => new MessageResult<bool>(new MessageError() { Code = msgError.Code, Description = msgError.Description }),
                        bResult => new MessageResult<bool>(bResult));
                });
        }
    }
}