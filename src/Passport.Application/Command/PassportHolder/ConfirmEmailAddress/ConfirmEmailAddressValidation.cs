using Passport.Abstraction.Result;
using Passport.Abstraction.Validation;
using Passport.Application.Default;
using Passport.Application.Interface;
using Passport.Application.Result;
using Passport.Application.Validation;

namespace Passport.Application.Command.PassportHolder.ConfirmEmailAddress
{
    internal sealed class ConfirmEmailAddressValidation : IValidation<ConfirmEmailAddressCommand>
    {
        private readonly TimeProvider prvTime;
        private readonly IPassportValidation srvValidation;

        private readonly IPassportHolderRepository repoHolder;

        public ConfirmEmailAddressValidation(IPassportHolderRepository repoHolder, IPassportValidation srvValidation, TimeProvider prvTime)
        {
            this.prvTime = prvTime;
            this.srvValidation = srvValidation;

            this.repoHolder = repoHolder;
        }

        async ValueTask<IMessageResult<bool>> IValidation<ConfirmEmailAddressCommand>.ValidateAsync(ConfirmEmailAddressCommand msgMessage, CancellationToken tknCancellation)
        {
            if (tknCancellation.IsCancellationRequested)
                return new MessageResult<bool>(DefaultMessageError.TaskAborted);

            srvValidation.ValidateEmailAddress(msgMessage.EmailAddress, "Email address");

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