using Mediator;
using Passport.Abstraction.Result;
using Passport.Application.Default;
using Passport.Application.Extension;
using Passport.Application.Interface;
using Passport.Application.Result;
using Passport.Application.Transfer;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace Passport.Application.Command.PassportToken.Create
{
    public sealed class CreatePassportTokenCommandHandler : ICommandHandler<CreatePassportTokenCommand, IMessageResult<Guid>>
    {
        private readonly TimeProvider prvTime;

        private readonly IPassportSetting ppSetting;

        private readonly IPassportTokenRepository repoToken;

        public CreatePassportTokenCommandHandler(
            TimeProvider prvTime,
            IPassportSetting ppSetting,
            IPassportTokenRepository repoToken)
        {
            this.prvTime = prvTime;
            this.ppSetting = ppSetting;
            this.repoToken = repoToken;
        }

        public async ValueTask<IMessageResult<Guid>> Handle(CreatePassportTokenCommand msgMessage, CancellationToken tknCancellation)
        {
            if (tknCancellation.IsCancellationRequested)
                return new MessageResult<Guid>(DefaultMessageError.TaskAborted);

            RepositoryResult<PassportTokenTransferObject> rsltToken = await repoToken.FindTokenByCredentialAsync(msgMessage.CredentialToVerify, prvTime.GetUtcNow(), tknCancellation);

            return await rsltToken.MatchAsync(
                msgError => new MessageResult<Guid>(new MessageError() { Code = msgError.Code, Description = msgError.Description }),
                async dtoPassportToken =>
                {
                    if (dtoPassportToken.Provider == msgMessage.CredentialToAdd.Provider)
                        return new MessageResult<Guid>(new MessageError() { Code = DomainError.Code.Method, Description = $"Token at provider {msgMessage.CredentialToAdd.Provider} does already exist." });

                    Domain.Aggregate.PassportToken? ppToken = Domain.Aggregate.PassportToken.Create(
                        dtExpiredAt: prvTime.GetUtcNow().Add(ppSetting.RefreshTokenExpiresAfterDuration),
                        guPassportId: dtoPassportToken.PassportId,
                        sProvider: msgMessage.CredentialToAdd.Provider,
                        bTwoFactorIsEnabled: false);

                    if (ppToken is null)
                        return new MessageResult<Guid>(new MessageError() { Code = DomainError.Code.Method, Description = "Token has not been created." });

                    RepositoryResult<bool> rsltTokenId = await repoToken.InsertAsync(ppToken.MapToTransferObject(), msgMessage.CredentialToAdd, prvTime.GetUtcNow(), tknCancellation);

                    return rsltTokenId.Match(
                        msgError => new MessageResult<Guid>(new MessageError() { Code = msgError.Code, Description = msgError.Description }),
                        bResult => new MessageResult<Guid>(ppToken.Id));
                });
        }
    }
}