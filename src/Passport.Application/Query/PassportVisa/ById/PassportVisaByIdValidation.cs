using Passport.Abstraction.Result;
using Passport.Abstraction.Validation;
using Passport.Application.Default;
using Passport.Application.Interface;
using Passport.Application.Result;
using Passport.Application.Validation;

namespace Passport.Application.Query.PassportVisa.ById
{
    internal class PassportVisaByIdValidation : IValidation<PassportVisaByIdQuery>
    {
        private readonly TimeProvider prvTime;
        private readonly IPassportValidation srvValidation;

        private readonly IPassportVisaRepository repoVisa;

        public PassportVisaByIdValidation(IPassportVisaRepository repoVisa, IPassportValidation srvValidation, TimeProvider prvTime)
        {
            this.prvTime = prvTime;
            this.srvValidation = srvValidation;

            this.repoVisa = repoVisa;
        }

        async ValueTask<IMessageResult<bool>> IValidation<PassportVisaByIdQuery>.ValidateAsync(PassportVisaByIdQuery msgMessage, CancellationToken tknCancellation)
        {
            if (tknCancellation.IsCancellationRequested)
                return new MessageResult<bool>(DefaultMessageError.TaskAborted);

            srvValidation.ValidateGuid(msgMessage.PassportVisaId, "Passport visa identifier");

            if (srvValidation.IsValid == true)
            {
                RepositoryResult<bool> rsltPassport = await repoVisa.ExistsAsync(msgMessage.PassportVisaId, tknCancellation);

                rsltPassport.Match(
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
