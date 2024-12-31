using Passport.Abstraction.Result;
using Passport.Abstraction.Validation;
using Passport.Application.Default;
using Passport.Application.Interface;
using Passport.Application.Result;
using Passport.Application.Validation;

namespace Passport.Application.Command.Authentication.Reset
{
    internal sealed class ResetRefreshTokenByPassportIdValidation : IValidation<ResetRefreshTokenByPassportIdCommand>
    {
        private readonly TimeProvider prvTime;
        private readonly IPassportValidation srvValidation;

        private readonly IPassportRepository repoPassport;

        public ResetRefreshTokenByPassportIdValidation(IPassportRepository repoPassport, IPassportValidation srvValidation, TimeProvider prvTime)
        {
            this.prvTime = prvTime;
            this.srvValidation = srvValidation;

            this.repoPassport = repoPassport;
        }

        async ValueTask<IMessageResult<bool>> IValidation<ResetRefreshTokenByPassportIdCommand>.ValidateAsync(ResetRefreshTokenByPassportIdCommand msgMessage, CancellationToken tknCancellation)
        {
            if (tknCancellation.IsCancellationRequested)
                return new MessageResult<bool>(DefaultMessageError.TaskAborted);

            srvValidation.ValidateProvider(msgMessage.Provider, "Provider");

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