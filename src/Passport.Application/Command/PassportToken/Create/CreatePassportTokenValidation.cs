using Passport.Abstraction.Result;
using Passport.Abstraction.Validation;
using Passport.Application.Default;
using Passport.Application.Interface;
using Passport.Application.Result;
using Passport.Application.Validation;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace Passport.Application.Command.PassportToken.Create
{
    internal sealed class CreatePassportTokenValidation : IValidation<CreatePassportTokenCommand>
    {
        private readonly TimeProvider prvTime;
        private readonly IPassportValidation srvValidation;

        private readonly IPassportTokenRepository repoToken;

        public CreatePassportTokenValidation(IPassportTokenRepository repoToken, IPassportValidation srvValidation, TimeProvider prvTime)
        {
            this.prvTime = prvTime;
            this.srvValidation = srvValidation;

            this.repoToken = repoToken;
        }

        async ValueTask<IMessageResult<bool>> IValidation<CreatePassportTokenCommand>.ValidateAsync(CreatePassportTokenCommand msgMessage, CancellationToken tknCancellation)
        {
            if (tknCancellation.IsCancellationRequested)
                return new MessageResult<bool>(DefaultMessageError.TaskAborted);

            srvValidation.ValidateCredential(msgMessage.CredentialToVerify.Credential, "Credential to verify");
            srvValidation.ValidateProvider(msgMessage.CredentialToVerify.Provider, "Provider to verify");
            srvValidation.ValidateSignature(msgMessage.CredentialToVerify.Signature, "Signature to verify");

            srvValidation.ValidateCredential(msgMessage.CredentialToAdd.Credential, "Credential to add");
            srvValidation.ValidateProvider(msgMessage.CredentialToAdd.Provider, "Provider to add");
            srvValidation.ValidateSignature(msgMessage.CredentialToAdd.Signature, "Signature to add");

            if (srvValidation.IsValid == true)
            {
                RepositoryResult<bool> rsltTokenToVerify = await repoToken.CredentialAtProviderExistsAsync(
                    sCredential: msgMessage.CredentialToVerify.Credential,
                    sProvider: msgMessage.CredentialToVerify.Provider,
                    tknCancellation: tknCancellation);

                rsltTokenToVerify.Match(
                    msgError => srvValidation.Add(new MessageError() { Code = msgError.Code, Description = msgError.Description }),
                    bResult =>
                    {
                        if (bResult == false)
                            srvValidation.Add(new MessageError() { Code = ValidationError.Code.Method, Description = $"Credential {msgMessage.CredentialToVerify.Credential} at {msgMessage.CredentialToVerify.Provider} does not exist." });

                        return bResult;
                    });

                RepositoryResult<bool> rsltTokenToAdd = await repoToken.CredentialAtProviderExistsAsync(
                    sCredential: msgMessage.CredentialToAdd.Credential,
                    sProvider: msgMessage.CredentialToAdd.Provider,
                    tknCancellation: tknCancellation);

                rsltTokenToAdd.Match(
                    msgError => srvValidation.Add(new MessageError() { Code = msgError.Code, Description = msgError.Description }),
                    bResult =>
                    {
                        if (bResult == true)
                            srvValidation.Add(new MessageError() { Code = ValidationError.Code.Method, Description = $"Credential {msgMessage.CredentialToAdd.Credential} at {msgMessage.CredentialToAdd.Provider} does already exist." });

                        return bResult;
                    });
            }

            return srvValidation.Match(
                        msgError => new MessageResult<bool>(msgError),
                        bResult => new MessageResult<bool>(bResult));
        }
    }
}