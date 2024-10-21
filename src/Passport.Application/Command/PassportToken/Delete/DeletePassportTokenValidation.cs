using Passport.Abstraction.Result;
using Passport.Abstraction.Validation;
using Passport.Application.Default;
using Passport.Application.Interface;
using Passport.Application.Result;
using Passport.Application.Validation;

namespace Passport.Application.Command.PassportToken.Delete
{
    internal sealed class DeletePassportTokenValidation : IValidation<DeletePassportTokenCommand>
    {
        private readonly TimeProvider prvTime;
        private readonly IPassportValidation srvValidation;

        private readonly IPassportTokenRepository repoToken;

        public DeletePassportTokenValidation(IPassportTokenRepository repoToken, IPassportValidation srvValidation, TimeProvider prvTime)
        {
            this.prvTime = prvTime;
            this.srvValidation = srvValidation;

            this.repoToken = repoToken;
        }

        async ValueTask<IMessageResult<bool>> IValidation<DeletePassportTokenCommand>.ValidateAsync(DeletePassportTokenCommand msgMessage, CancellationToken tknCancellation)
        {
            if (tknCancellation.IsCancellationRequested)
                return new MessageResult<bool>(DefaultMessageError.TaskAborted);

            srvValidation.ValidateCredential(msgMessage.CredentialToRemove.Credential, "Credential");
            srvValidation.ValidateProvider(msgMessage.CredentialToRemove.Provider, "Provider");

            if (srvValidation.IsValid == true)
            {
                RepositoryResult<bool> rsltToken = await repoToken.CredentialAtProviderExistsAsync(msgMessage.CredentialToRemove.Credential, msgMessage.CredentialToRemove.Provider, tknCancellation);

                rsltToken.Match(
                    msgError => srvValidation.Add(new MessageError() { Code = msgError.Code, Description = msgError.Description }),
                    bResult =>
                    {
                        if (bResult == false)
                            srvValidation.Add(new MessageError() { Code = ValidationError.Code.Method, Description = $"Credential at provider {msgMessage.CredentialToRemove.Provider} does not exist." });

                        return bResult;
                    });
            }

            return srvValidation.Match(
                        msgError => new MessageResult<bool>(msgError),
                        bResult => new MessageResult<bool>(bResult));
        }
    }
}