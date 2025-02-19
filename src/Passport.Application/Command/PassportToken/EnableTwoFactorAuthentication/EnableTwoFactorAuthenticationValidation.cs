using Passport.Abstraction.Result;
using Passport.Abstraction.Validation;
using Passport.Application.Default;
using Passport.Application.Interface;
using Passport.Application.Result;
using Passport.Application.Validation;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace Passport.Application.Command.PassportToken.EnableTwoFactorAuthentication
{
    internal sealed class EnableTwoFactorAuthenticationValidation : IValidation<EnableTwoFactorAuthenticationCommand>
    {
        private readonly TimeProvider prvTime;
        private readonly IPassportValidation srvValidation;

        private readonly IPassportTokenRepository repoToken;

        public EnableTwoFactorAuthenticationValidation(IPassportTokenRepository repoToken, IPassportValidation srvValidation, TimeProvider prvTime)
        {
            this.prvTime = prvTime;
            this.srvValidation = srvValidation;

            this.repoToken = repoToken;
        }

        async ValueTask<IMessageResult<bool>> IValidation<EnableTwoFactorAuthenticationCommand>.ValidateAsync(EnableTwoFactorAuthenticationCommand msgMessage, CancellationToken tknCancellation)
        {
            if (tknCancellation.IsCancellationRequested)
                return new MessageResult<bool>(DefaultMessageError.TaskAborted);

            srvValidation.ValidateCredential(msgMessage.CredentialToVerify.Credential, "Credential");
            srvValidation.ValidateProvider(msgMessage.CredentialToVerify.Provider, "Provider");
            srvValidation.ValidateSignature(msgMessage.CredentialToVerify.Signature, "Signature");

            if (srvValidation.IsValid == true)
            {
                RepositoryResult<bool> rsltToken = await repoToken.CredentialAtProviderExistsAsync(
                    sCredential: msgMessage.CredentialToVerify.Credential,
                    sProvider: msgMessage.CredentialToVerify.Provider,
                    tknCancellation: tknCancellation);

                rsltToken.Match(
                    msgError => srvValidation.Add(new MessageError() { Code = msgError.Code, Description = msgError.Description }),
                    bResult =>
                    {
                        if (bResult == false)
                            srvValidation.Add(new MessageError() { Code = ValidationError.Code.Method, Description = $"Credential {msgMessage.CredentialToVerify.Credential} does not exist." });

                        return bResult;
                    });
            }

            return srvValidation.Match(
                        msgError => new MessageResult<bool>(msgError),
                        bResult => new MessageResult<bool>(bResult));
        }
    }
}
