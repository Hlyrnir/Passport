using Mediator;
using Passport.Abstraction.Result;
using Passport.Application.Default;
using Passport.Application.Interface;
using Passport.Application.Result;

namespace Passport.Application.Command.Authentication.Reset
{
    public sealed class ResetRefreshTokenByPassportIdCommandHandler : ICommandHandler<ResetRefreshTokenByPassportIdCommand, IMessageResult<bool>>
    {
        private readonly TimeProvider prvTime;

        private readonly IPassportTokenRepository repoToken;

        public ResetRefreshTokenByPassportIdCommandHandler(
            TimeProvider prvTime,
            IPassportTokenRepository repoToken)
        {
            this.prvTime = prvTime;
            this.repoToken = repoToken;
        }

        public async ValueTask<IMessageResult<bool>> Handle(ResetRefreshTokenByPassportIdCommand msgMessage, CancellationToken tknCancellation)
        {
            if (tknCancellation.IsCancellationRequested)
                return new MessageResult<bool>(DefaultMessageError.TaskAborted);

            RepositoryResult<bool> pprReset = await repoToken.ResetRefreshTokenAsync(msgMessage.PassportId, msgMessage.Provider, prvTime.GetUtcNow(), tknCancellation);

            return pprReset.Match(
                msgError => new MessageResult<bool>(new MessageError() { Code = msgError.Code, Description = msgError.Description }),
                bResult => new MessageResult<bool>(bResult));
        }
    }
}