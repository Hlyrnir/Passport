using Passport.Abstraction.Result;
using Passport.Abstraction.Validation;
using Passport.Application.Default;
using Passport.Application.Interface;
using Passport.Application.Result;
using Passport.Application.Validation;

namespace Passport.Application.Command.Passport.Register
{
    internal sealed class RegisterPassportValidation : IValidation<RegisterPassportCommand>
    {
        private readonly TimeProvider prvTime;
        private readonly IPassportValidation srvValidation;

        private readonly IPassportTokenRepository repoToken;

        public RegisterPassportValidation(IPassportTokenRepository repoToken, IPassportValidation srvValidation, TimeProvider prvTime)
        {
            this.prvTime = prvTime;
            this.srvValidation = srvValidation;

            this.repoToken = repoToken;
        }

        async ValueTask<IMessageResult<bool>> IValidation<RegisterPassportCommand>.ValidateAsync(RegisterPassportCommand msgMessage, CancellationToken tknCancellation)
        {
            if (tknCancellation.IsCancellationRequested)
                return new MessageResult<bool>(DefaultMessageError.TaskAborted);

            srvValidation.ValidateCredential(msgMessage.CredentialToRegister.Credential, "Credential");
            srvValidation.ValidateProvider(msgMessage.CredentialToRegister.Provider, "Provider");
            srvValidation.ValidateSignature(msgMessage.CredentialToRegister.Signature, "Signature");

            srvValidation.ValidateEmailAddress(msgMessage.EmailAddress, "E-mail address");
            srvValidation.ValidatePhoneNumber(msgMessage.PhoneNumber, "Phone number");

            if (srvValidation.IsValid == true)
            {
                RepositoryResult<bool> rsltCredential = await repoToken.CredentialAtProviderExistsAsync(
                    sCredential: msgMessage.CredentialToRegister.Credential,
                    sProvider: msgMessage.CredentialToRegister.Provider,
                    tknCancellation: tknCancellation);

                rsltCredential.Match(
                    msgError => srvValidation.Add(new MessageError() { Code = msgError.Code, Description = msgError.Description }),
                    bResult =>
                    {
                        if (bResult == true)
                            srvValidation.Add(new MessageError() { Code = ValidationError.Code.Method, Description = $"Credential {msgMessage.CredentialToRegister.Credential} does already exist." });

                        return bResult;
                    });
            }

            return srvValidation.Match(
                    msgError => new MessageResult<bool>(msgError),
                    bResult => new MessageResult<bool>(bResult));
        }
    }
}
