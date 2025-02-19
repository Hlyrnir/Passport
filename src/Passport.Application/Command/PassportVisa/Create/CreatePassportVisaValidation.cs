using Passport.Abstraction.Result;
using Passport.Abstraction.Validation;
using Passport.Application.Default;
using Passport.Application.Interface;
using Passport.Application.Result;
using Passport.Application.Validation;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace Passport.Application.Command.PassportVisa.Create
{
    internal sealed class CreatePassportVisaValidation : IValidation<CreatePassportVisaCommand>
    {
        private readonly TimeProvider prvTime;
        private readonly IPassportValidation srvValidation;

        private readonly IPassportVisaRepository repoVisa;

        public CreatePassportVisaValidation(IPassportVisaRepository repoVisa, IPassportValidation srvValidation, TimeProvider prvTime)
        {
            this.prvTime = prvTime;
            this.srvValidation = srvValidation;

            this.repoVisa = repoVisa;
        }

        async ValueTask<IMessageResult<bool>> IValidation<CreatePassportVisaCommand>.ValidateAsync(CreatePassportVisaCommand msgMessage, CancellationToken tknCancellation)
        {
            if (tknCancellation.IsCancellationRequested)
                return new MessageResult<bool>(DefaultMessageError.TaskAborted);

            if (string.IsNullOrWhiteSpace(msgMessage.Name) == true)
                srvValidation.Add(new MessageError() { Code = ValidationError.Code.Method, Description = "Name is invalid (empty)." });

            if (msgMessage.Level < 0)
                srvValidation.Add(new MessageError() { Code = ValidationError.Code.Method, Description = "Level must be greater than or equal to zero." });

            if (srvValidation.IsValid == true)
            {
                RepositoryResult<bool> rsltVisa = await repoVisa.ByNameAtLevelExistsAsync(
                    sName: msgMessage.Name,
                    iLevel: msgMessage.Level,
                    tknCancellation: tknCancellation);

                rsltVisa.Match(
                    msgError => srvValidation.Add(new MessageError() { Code = msgError.Code, Description = msgError.Description }),
                    bResult =>
                    {
                        if (bResult == true)
                            srvValidation.Add(new MessageError() { Code = ValidationError.Code.Method, Description = $"Visa of name {msgMessage.Name} at level {msgMessage.Level} does already exist." });

                        return bResult;
                    });
            }

            return srvValidation.Match(
                    msgError => new MessageResult<bool>(msgError),
                    bResult => new MessageResult<bool>(bResult));
        }
    }
}
