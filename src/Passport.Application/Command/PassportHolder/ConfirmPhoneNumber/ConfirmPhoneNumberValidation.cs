using Passport.Abstraction.Result;
using Passport.Abstraction.Validation;
using Passport.Application.Default;
using Passport.Application.Interface;
using Passport.Application.Result;
using Passport.Application.Validation;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace Passport.Application.Command.PassportHolder.ConfirmPhoneNumber
{
    internal sealed class ConfirmPhoneNumberValidation : IValidation<ConfirmPhoneNumberCommand>
    {
        private readonly TimeProvider prvTime;
        private readonly IPassportValidation srvValidation;

        private readonly IPassportHolderRepository repoHolder;

        public ConfirmPhoneNumberValidation(IPassportHolderRepository repoHolder, IPassportValidation srvValidation, TimeProvider prvTime)
        {
            this.prvTime = prvTime;
            this.srvValidation = srvValidation;

            this.repoHolder = repoHolder;
        }

        async ValueTask<IMessageResult<bool>> IValidation<ConfirmPhoneNumberCommand>.ValidateAsync(ConfirmPhoneNumberCommand msgMessage, CancellationToken tknCancellation)
        {
            if (tknCancellation.IsCancellationRequested)
                return new MessageResult<bool>(DefaultMessageError.TaskAborted);

            srvValidation.ValidatePhoneNumber(msgMessage.PhoneNumber, "Phone number");

            RepositoryResult<bool> rsltHolder = await repoHolder.ExistsAsync(msgMessage.PassportHolderId, tknCancellation);

            rsltHolder.Match(
                msgError => srvValidation.Add(new MessageError() { Code = msgError.Code, Description = msgError.Description }),
                bResult =>
                {
                    if (bResult == false)
                        srvValidation.Add(new MessageError() { Code = ValidationError.Code.Method, Description = $"Passport holder {msgMessage.PassportHolderId} does not exist." });

                    return bResult;
                });

            return srvValidation.Match(
                    msgError => new MessageResult<bool>(msgError),
                    bResult => new MessageResult<bool>(bResult));
        }
    }
}
