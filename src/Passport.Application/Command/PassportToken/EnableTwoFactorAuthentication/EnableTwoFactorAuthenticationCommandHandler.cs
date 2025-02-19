using Mediator;
using Passport.Abstraction.Result;
using Passport.Application.Default;
using Passport.Application.Interface;
using Passport.Application.Result;
using Passport.Application.Transfer;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace Passport.Application.Command.PassportToken.EnableTwoFactorAuthentication
{
    public sealed class EnableTwoFactorAuthenticationCommandHandler : ICommandHandler<EnableTwoFactorAuthenticationCommand, IMessageResult<bool>>
    {
        private readonly TimeProvider prvTime;

        private readonly IPassportTokenRepository repoToken;

        public EnableTwoFactorAuthenticationCommandHandler(
            TimeProvider prvTime,
            IPassportTokenRepository repoToken)
        {
            this.prvTime = prvTime;
            this.repoToken = repoToken;
        }

        public async ValueTask<IMessageResult<bool>> Handle(EnableTwoFactorAuthenticationCommand msgMessage, CancellationToken tknCancellation)
        {
            if (tknCancellation.IsCancellationRequested)
                return new MessageResult<bool>(DefaultMessageError.TaskAborted);

            RepositoryResult<PassportTokenTransferObject> rsltToken = await repoToken.FindTokenByCredentialAsync(msgMessage.CredentialToVerify, prvTime.GetUtcNow(), tknCancellation);

            return await rsltToken.MatchAsync(
                msgError => new MessageResult<bool>(new MessageError() { Code = msgError.Code, Description = msgError.Description }),
                async dtoPassportToken =>
                {
                    if (dtoPassportToken.TwoFactorIsEnabled == true)
                        return new MessageResult<bool>(new MessageError() { Code = DomainError.Code.Method, Description = "Two factor authentication is already enabled." });

                    RepositoryResult<bool> rsltEnable = await repoToken.EnableTwoFactorAuthenticationAsync(dtoPassportToken, msgMessage.TwoFactorIsEnabled, prvTime.GetUtcNow(), tknCancellation);

                    return rsltEnable.Match(
                        msgError => new MessageResult<bool>(new MessageError() { Code = msgError.Code, Description = msgError.Description }),
                        bResult => new MessageResult<bool>(bResult));
                });
        }
    }
}