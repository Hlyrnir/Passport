using Passport.Abstraction.Result;
using Passport.Abstraction.Validation;
using Passport.Application.Default;
using Passport.Application.Interface;
using Passport.Application.Result;
using Passport.Application.Validation;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace Passport.Application.Command.PassportVisa.Delete
{
    internal sealed class DeletePassportVisaValidation : IValidation<DeletePassportVisaCommand>
    {
        private readonly TimeProvider prvTime;
        private readonly IPassportValidation srvValidation;

        private readonly IPassportVisaRepository repoVisa;

        public DeletePassportVisaValidation(IPassportVisaRepository repoVisa, IPassportValidation srvValidation, TimeProvider prvTime)
        {
            this.prvTime = prvTime;
            this.srvValidation = srvValidation;

            this.repoVisa = repoVisa;
        }

        async ValueTask<IMessageResult<bool>> IValidation<DeletePassportVisaCommand>.ValidateAsync(DeletePassportVisaCommand msgMessage, CancellationToken tknCancellation)
        {
            if (tknCancellation.IsCancellationRequested)
                return new MessageResult<bool>(DefaultMessageError.TaskAborted);

            srvValidation.ValidateGuid(msgMessage.PassportVisaId, "Passport visa identifier");

            if (srvValidation.IsValid == true)
            {
                RepositoryResult<bool> rsltVisa = await repoVisa.ExistsAsync(msgMessage.PassportVisaId, tknCancellation);

                rsltVisa.Match(
                    msgError => srvValidation.Add(new MessageError() { Code = msgError.Code, Description = msgError.Description }),
                    bResult =>
                    {
                        if (bResult == false)
                            srvValidation.Add(new MessageError() { Code = ValidationError.Code.Method, Description = $"Passport visa {msgMessage.PassportVisaId} does not exist." });

                        return bResult;
                    });
            }

            return srvValidation.Match(
                    msgError => new MessageResult<bool>(msgError),
                    bResult => new MessageResult<bool>(bResult));
        }
    }
}
