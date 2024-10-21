using Mediator;
using Microsoft.Extensions.DependencyInjection;
using Passport.Abstraction.Result;
using Passport.Application.Default;
using Passport.Application.Extension;
using Passport.Application.Interface;
using Passport.Application.Result;
using Passport.Domain;

namespace Passport.Application.Command.Passport.Register
{
    public sealed class RegisterPassportCommandHandler : ICommandHandler<RegisterPassportCommand, IMessageResult<Guid>>
    {
        private readonly TimeProvider prvTime;

        private readonly IUnitOfWork uowUnitOfWork;
        private readonly IPassportSetting ppSetting;

        private readonly IPassportRepository repoPassport;
        private readonly IPassportHolderRepository repoHolder;
        private readonly IPassportTokenRepository repoToken;

        public RegisterPassportCommandHandler(
            TimeProvider prvTime,
            [FromKeyedServices(DefaultKeyedServiceName.UnitOfWork)] IUnitOfWork uowUnitOfWork,
            IPassportSetting ppSetting,
            IPassportRepository repoPassport,
            IPassportHolderRepository repoHolder,
            IPassportTokenRepository repoToken)
        {
            this.prvTime = prvTime;
            this.uowUnitOfWork = uowUnitOfWork;
            this.ppSetting = ppSetting;
            this.repoPassport = repoPassport;
            this.repoToken = repoToken;
            this.repoHolder = repoHolder;
        }

        public async ValueTask<IMessageResult<Guid>> Handle(RegisterPassportCommand cmdRegisterPassport, CancellationToken tknCancellation)
        {
            if (tknCancellation.IsCancellationRequested)
                return new MessageResult<Guid>(DefaultMessageError.TaskAborted);

            Domain.Aggregate.PassportHolder? ppHolder = Domain.Aggregate.PassportHolder.Create(
                sCultureName: cmdRegisterPassport.CultureName,
                sEmailAddress: cmdRegisterPassport.EmailAddress,
                sFirstName: cmdRegisterPassport.FirstName,
                sLastName: cmdRegisterPassport.LastName,
                sPhoneNumber: cmdRegisterPassport.PhoneNumber,
                ppSetting: ppSetting);

            if (ppHolder is null)
                return new MessageResult<Guid>(new MessageError() { Code = DomainError.Code.Method, Description = "Holder has not been created." });

            Domain.Aggregate.Passport? ppPassport = Domain.Aggregate.Passport.Create(
                dtExpiredAt: prvTime.GetUtcNow().Add(ppSetting.PassportExpiresAfterDuration),
                guHolderId: ppHolder.Id,
                guIssuedBy: cmdRegisterPassport.IssuedBy,
                dtLastCheckedAt: prvTime.GetUtcNow());

            if (ppPassport is null)
                return new MessageResult<Guid>(new MessageError() { Code = DomainError.Code.Method, Description = "Passport has not been created." });

            Domain.Aggregate.PassportToken? ppToken = Domain.Aggregate.PassportToken.Create(
                dtExpiredAt: prvTime.GetUtcNow().Add(ppSetting.RefreshTokenExpiresAfterDuration),
                guPassportId: ppPassport.Id,
                sProvider: cmdRegisterPassport.CredentialToRegister.Provider,
                bTwoFactorIsEnabled: false);

            if (ppToken is null)
                return new MessageResult<Guid>(new MessageError() { Code = DomainError.Code.Method, Description = "Token has not been created." });

            bool bIsCommited = false;

            await uowUnitOfWork.TransactionAsync(async () =>
            {
                await repoHolder.InsertAsync(ppHolder.MapToTransferObject(), prvTime.GetUtcNow(), tknCancellation);
                await repoPassport.InsertAsync(ppPassport.MapToTransferObject(), prvTime.GetUtcNow(), tknCancellation);
                await repoToken.InsertAsync(ppToken.MapToTransferObject(), cmdRegisterPassport.CredentialToRegister, prvTime.GetUtcNow(), tknCancellation);

                bIsCommited = uowUnitOfWork.TryCommit();

                if (bIsCommited == false)
                    uowUnitOfWork.TryRollback();
            });

            if (bIsCommited == false)
                return new MessageResult<Guid>(new MessageError() { Code = DomainError.Code.Method, Description = "Transaction has not been committed." });

            return new MessageResult<Guid>(ppPassport.Id);
        }
    }
}
