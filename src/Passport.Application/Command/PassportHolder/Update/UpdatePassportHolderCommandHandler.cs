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

namespace Passport.Application.Command.PassportHolder.Update
{
    public sealed class UpdatePassportHolderCommandHandler : ICommandHandler<UpdatePassportHolderCommand, IMessageResult<bool>>
    {
        private readonly TimeProvider prvTime;

        private readonly IPassportSetting ppSetting;

        private readonly IPassportHolderRepository repoHolder;

        public UpdatePassportHolderCommandHandler(
            TimeProvider prvTime,
            IPassportSetting ppSetting,
            IPassportHolderRepository repoHolder)
        {
            this.prvTime = prvTime;
            this.ppSetting = ppSetting;
            this.repoHolder = repoHolder;
        }

        public async ValueTask<IMessageResult<bool>> Handle(UpdatePassportHolderCommand msgMessage, CancellationToken tknCancellation)
        {
            if (tknCancellation.IsCancellationRequested)
                return new MessageResult<bool>(DefaultMessageError.TaskAborted);

            RepositoryResult<PassportHolderTransferObject> rsltHolder = await repoHolder.FindByIdAsync(msgMessage.PassportHolderId, tknCancellation);

            return await rsltHolder.MatchAsync(
                msgError => new MessageResult<bool>(new MessageError() { Code = msgError.Code, Description = msgError.Description }),
                async dtoPassportHolder =>
                {
                    if (dtoPassportHolder.ConcurrencyStamp != msgMessage.ConcurrencyStamp)
                        return new MessageResult<bool>(DefaultMessageError.ConcurrencyViolation);

                    Domain.ValueObject.PassportHolderSetting ppHolderSetting = ppSetting.MapToPassportHolderSetting();
                    Domain.Aggregate.PassportHolder? ppHolder = dtoPassportHolder.Initialize(ppHolderSetting);

                    if (ppHolder is null)
                        return new MessageResult<bool>(DomainError.InitializationHasFailed);

                    if (ppHolder.CultureName != msgMessage.CultureName)
                    {
                        if (ppHolder.TryChangeCultureName(msgMessage.CultureName) == false)
                            return new MessageResult<bool>(new MessageError() { Code = DomainError.Code.Method, Description = "Culture name could not be changed." });
                    }

                    if (ppHolder.EmailAddress != msgMessage.EmailAddress)
                    {
                        if (ppHolder.TryChangeEmailAddress(msgMessage.EmailAddress, ppHolderSetting) == false)
                            return new MessageResult<bool>(new MessageError() { Code = DomainError.Code.Method, Description = "Email address could not be changed." });
                    }

                    if (ppHolder.PhoneNumber != msgMessage.PhoneNumber)
                    {
                        if (ppHolder.TryChangePhoneNumber(msgMessage.PhoneNumber, ppHolderSetting) == false)
                            return new MessageResult<bool>(new MessageError() { Code = DomainError.Code.Method, Description = "Phone number could not be changed." });
                    }

                    if (ppHolder.FirstName != msgMessage.FirstName)
                        ppHolder.FirstName = msgMessage.FirstName;

                    if (ppHolder.LastName != msgMessage.LastName)
                        ppHolder.LastName = msgMessage.LastName;

                    RepositoryResult<bool> rsltUpdate = await repoHolder.UpdateAsync(ppHolder.MapToTransferObject(), prvTime.GetUtcNow(), tknCancellation);

                    return rsltUpdate.Match(
                        msgError => new MessageResult<bool>(new MessageError() { Code = msgError.Code, Description = msgError.Description }),
                        bResult => new MessageResult<bool>(bResult));
                });
        }
    }
}
