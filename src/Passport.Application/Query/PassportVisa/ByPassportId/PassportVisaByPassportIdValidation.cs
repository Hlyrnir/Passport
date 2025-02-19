using Passport.Abstraction.Result;
using Passport.Abstraction.Validation;
using Passport.Application.Default;
using Passport.Application.Interface;
using Passport.Application.Result;
using Passport.Application.Validation;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace Passport.Application.Query.PassportVisa.ByPassportId
{
    internal sealed class PassportVisaByPassportIdValidation : IValidation<PassportVisaByPassportIdQuery>
    {
        private readonly TimeProvider prvTime;
        private readonly IPassportValidation srvValidation;

        private readonly IPassportRepository repoPassport;

        public PassportVisaByPassportIdValidation(IPassportRepository repoPassport, IPassportValidation srvValidation, TimeProvider prvTime)
        {
            this.prvTime = prvTime;
            this.srvValidation = srvValidation;

            this.repoPassport = repoPassport;
        }

        async ValueTask<IMessageResult<bool>> IValidation<PassportVisaByPassportIdQuery>.ValidateAsync(PassportVisaByPassportIdQuery msgMessage, CancellationToken tknCancellation)
        {
            if (tknCancellation.IsCancellationRequested)
                return new MessageResult<bool>(DefaultMessageError.TaskAborted);

            srvValidation.ValidateGuid(msgMessage.PassportIdToFind, "Passport identifier");

            if (srvValidation.IsValid == true)
            {
                RepositoryResult<bool> rsltPassport = await repoPassport.ExistsAsync(msgMessage.PassportIdToFind, tknCancellation);

                rsltPassport.Match(
                    msgError => srvValidation.Add(new MessageError() { Code = msgError.Code, Description = msgError.Description }),
                    bResult =>
                    {
                        if (bResult == false)
                            srvValidation.Add(new MessageError() { Code = ValidationError.Code.Method, Description = $"Passport {msgMessage.PassportIdToFind} does not exist." });

                        return bResult;
                    });
            }

            return srvValidation.Match(
                    msgError => new MessageResult<bool>(msgError),
                    bResult => new MessageResult<bool>(bResult));
        }
    }
}
