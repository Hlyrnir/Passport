using Passport.Abstraction.Result;
using Passport.Abstraction.Validation;
using Passport.Application.Default;
using Passport.Application.Interface;
using Passport.Application.Result;
using Passport.Application.Validation;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace Passport.Application.Command.Passport.Update
{
    internal sealed class UpdatePassportValidation : IValidation<UpdatePassportCommand>
    {
        private readonly TimeProvider prvTime;
        private readonly IPassportValidation srvValidation;

        private readonly IPassportRepository repoPassport;
        private readonly IPassportVisaRepository repoVisa;

        public UpdatePassportValidation(IPassportRepository repoPassport, IPassportVisaRepository repoVisa, IPassportValidation srvValidation, TimeProvider prvTime)
        {
            this.prvTime = prvTime;
            this.srvValidation = srvValidation;

            this.repoPassport = repoPassport;
            this.repoVisa = repoVisa;
        }

        async ValueTask<IMessageResult<bool>> IValidation<UpdatePassportCommand>.ValidateAsync(UpdatePassportCommand msgMessage, CancellationToken tknCancellation)
        {
            if (tknCancellation.IsCancellationRequested)
                return new MessageResult<bool>(DefaultMessageError.TaskAborted);

            if (DateTimeOffset.Compare(msgMessage.ExpiredAt, prvTime.GetUtcNow()) < 0)
                srvValidation.Add(new MessageError() { Code = ValidationError.Code.Method, Description = "Expiration date must be in the future." });

            if (DateTimeOffset.Compare(msgMessage.LastCheckedAt, prvTime.GetUtcNow()) > 0)
                srvValidation.Add(new MessageError() { Code = ValidationError.Code.Method, Description = "Last checked date must be in the past." });

            if (srvValidation.IsValid == true)
            {
                RepositoryResult<bool> rsltPassport = await repoPassport.ExistsAsync(msgMessage.PassportIdToUpdate, tknCancellation);

                rsltPassport.Match(
                    msgError => srvValidation.Add(new MessageError() { Code = msgError.Code, Description = msgError.Description }),
                    bResult =>
                    {
                        if (bResult == false)
                            srvValidation.Add(new MessageError() { Code = ValidationError.Code.Method, Description = $"Passport {msgMessage.PassportIdToUpdate} does not exist." });

                        return bResult;
                    });

                foreach (Guid guVisaId in msgMessage.PassportVisaId)
                {
                    RepositoryResult<bool> rsltVisa = await repoVisa.ExistsAsync(guVisaId, tknCancellation);

                    rsltVisa.Match(
                        msgError => srvValidation.Add(new MessageError() { Code = msgError.Code, Description = msgError.Description }),
                        bResult =>
                        {
                            if (bResult == false)
                                srvValidation.Add(new MessageError() { Code = ValidationError.Code.Method, Description = $"Visa {guVisaId} does not exists." });

                            return bResult;
                        });
                }
            }

            return srvValidation.Match(
                    msgError => new MessageResult<bool>(msgError),
                    bResult => new MessageResult<bool>(bResult));
        }
    }
}
