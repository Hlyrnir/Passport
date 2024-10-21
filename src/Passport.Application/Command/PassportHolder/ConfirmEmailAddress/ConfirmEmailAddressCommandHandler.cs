﻿using Mediator;
using Passport.Abstraction.Result;
using Passport.Application.Default;
using Passport.Application.Extension;
using Passport.Application.Interface;
using Passport.Application.Result;
using Passport.Application.Transfer;
using Passport.Domain;

namespace Passport.Application.Command.PassportHolder.ConfirmEmailAddress
{
    public sealed class ConfirmEmailAddressCommandHandler : ICommandHandler<ConfirmEmailAddressCommand, IMessageResult<bool>>
    {
        private readonly TimeProvider prvTime;

        private readonly IPassportSetting ppSetting;

        private readonly IPassportHolderRepository repoHolder;

        public ConfirmEmailAddressCommandHandler(
            TimeProvider prvTime,
            IPassportSetting ppSetting,
            IPassportHolderRepository repoHolder)
        {
            this.prvTime = prvTime;
            this.ppSetting = ppSetting;
            this.repoHolder = repoHolder;
        }

        public async ValueTask<IMessageResult<bool>> Handle(ConfirmEmailAddressCommand msgMessage, CancellationToken tknCancellation)
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

                    Domain.Aggregate.PassportHolder? ppHolder = dtoPassportHolder.Initialize(ppSetting);

                    if (ppHolder is null)
                        return new MessageResult<bool>(DomainError.InitializationHasFailed);

                    if (ppHolder.EmailAddressIsConfirmed == false)
                    {
                        if (ppHolder.TryConfirmEmailAddress(msgMessage.EmailAddress, ppSetting) == false)
                            return new MessageResult<bool>(new MessageError() { Code = DomainError.Code.Method, Description = "Email address could not be confirmed." });

                        RepositoryResult<bool> rsltUpdate = await repoHolder.UpdateAsync(ppHolder.MapToTransferObject(), prvTime.GetUtcNow(), tknCancellation);

                        return rsltUpdate.Match(
                            msgError => new MessageResult<bool>(new MessageError() { Code = msgError.Code, Description = msgError.Description }),
                            bResult => new MessageResult<bool>(bResult));
                    }

                    return new MessageResult<bool>(new MessageError() { Code = DomainError.Code.Method, Description = "Email address is already confirmed." });
                });
        }
    }
}