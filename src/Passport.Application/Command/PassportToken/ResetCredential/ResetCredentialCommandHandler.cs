using Mediator;
using Microsoft.Extensions.DependencyInjection;
using Passport.Abstraction.Result;
using Passport.Application.Default;
using Passport.Application.Extension;
using Passport.Application.Interface;
using Passport.Application.Result;
using Passport.Application.Transfer;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace Passport.Application.Command.PassportToken.ResetCredential
{
    public sealed class ResetCredentialCommandHandler : ICommandHandler<ResetCredentialCommand, IMessageResult<bool>>
    {
        private readonly TimeProvider prvTime;

        private readonly IUnitOfWork uowUnitOfWork;
        private readonly IPassportSetting ppSetting;

        private readonly IPassportRepository repoPassport;
        private readonly IPassportTokenRepository repoToken;

        public ResetCredentialCommandHandler(
            TimeProvider prvTime,
            [FromKeyedServices(DefaultKeyedServiceName.UnitOfWork)] IUnitOfWork uowUnitOfWork,
            IPassportSetting ppSetting,
            IPassportRepository repoPassport,
            IPassportTokenRepository repoToken)
        {
            this.prvTime = prvTime;
            this.uowUnitOfWork = uowUnitOfWork;
            this.ppSetting = ppSetting;
            this.repoPassport = repoPassport;
            this.repoToken = repoToken;
        }

        public async ValueTask<IMessageResult<bool>> Handle(ResetCredentialCommand msgMessage, CancellationToken tknCancellation)
        {
            if (tknCancellation.IsCancellationRequested)
                return new MessageResult<bool>(DefaultMessageError.TaskAborted);

            RepositoryResult<PassportTokenTransferObject> rsltToken = await repoToken.FindTokenByCredentialAsync(msgMessage.CredentialToVerify, prvTime.GetUtcNow(), tknCancellation);

            return await rsltToken.MatchAsync(
                msgError => new MessageResult<bool>(new MessageError() { Code = msgError.Code, Description = msgError.Description }),
                async ppToken =>
                {
                    RepositoryResult<PassportTransferObject> rsltPassport = await repoPassport.FindByIdAsync(ppToken.PassportId, tknCancellation);

                    return await rsltPassport.MatchAsync(
                        msgError => new MessageResult<bool>(new MessageError() { Code = msgError.Code, Description = msgError.Description }),
                        async dtoPassport =>
                        {
                            Domain.Aggregate.Passport? ppPassport = dtoPassport.Initialize();

                            if (ppPassport is null)
                                return new MessageResult<bool>(DomainError.InitializationHasFailed);

                            ppPassport.TryExtendTerm(prvTime.GetUtcNow().Add(ppSetting.PassportExpiresAfterDuration), prvTime.GetUtcNow(), msgMessage.RestrictedPassportId);

                            bool bIsCommited = false;

                            await uowUnitOfWork.TransactionAsync(async () =>
                            {
                                await repoToken.ResetCredentialAsync(ppToken, msgMessage.CredentialToApply, prvTime.GetUtcNow(), tknCancellation);
                                await repoPassport.UpdateAsync(ppPassport.MapToTransferObject(), prvTime.GetUtcNow(), tknCancellation);

                                bIsCommited = uowUnitOfWork.TryCommit();

                                if (bIsCommited == false)
                                    uowUnitOfWork.TryRollback();
                            });

                            if (bIsCommited == false)
                                return new MessageResult<bool>(new MessageError() { Code = DomainError.Code.Method, Description = "Transaction has not been committed." });

                            return new MessageResult<bool>(true);
                        });
                });
        }
    }
}
