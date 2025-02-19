using Passport.Abstraction.Result;
using Passport.Abstraction.Validation;
using Passport.Application.Default;
using Passport.Application.Interface;
using Passport.Application.Result;
using Passport.Application.Validation;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace Passport.Application.Query.Passport.ById
{
    internal sealed class PassportByIdValidation : IValidation<PassportByIdQuery>
    {
        private readonly TimeProvider prvTime;
        private readonly IPassportValidation srvValidation;

        private readonly IPassportRepository repoPassport;

        public PassportByIdValidation(IPassportRepository repoPassport, IPassportValidation srvValidation, TimeProvider prvTime)
        {
            this.prvTime = prvTime;
            this.srvValidation = srvValidation;

            this.repoPassport = repoPassport;
        }

        async ValueTask<IMessageResult<bool>> IValidation<PassportByIdQuery>.ValidateAsync(PassportByIdQuery msgMessage, CancellationToken tknCancellation)
        {
            if (tknCancellation.IsCancellationRequested)
                return new MessageResult<bool>(DefaultMessageError.TaskAborted);

            srvValidation.ValidateGuid(msgMessage.PassportId, "Passport identifier");

            if (srvValidation.IsValid == true)
            {
                RepositoryResult<bool> rsltPassport = await repoPassport.ExistsAsync(msgMessage.PassportId, tknCancellation);

                rsltPassport.Match(
                    msgError => srvValidation.Add(new MessageError() { Code = msgError.Code, Description = msgError.Description }),
                    bResult =>
                    {
                        if (bResult == false)
                            srvValidation.Add(new MessageError() { Code = ValidationError.Code.Method, Description = $"Passport {msgMessage.PassportId} does not exist." });

                        return bResult;
                    });
            }

            return srvValidation.Match(
                    msgError => new MessageResult<bool>(msgError),
                    bResult => new MessageResult<bool>(bResult));
        }
    }
}
