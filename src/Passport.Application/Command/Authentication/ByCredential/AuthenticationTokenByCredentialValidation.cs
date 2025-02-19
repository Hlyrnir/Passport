using Passport.Abstraction.Result;
using Passport.Abstraction.Validation;
using Passport.Application.Default;
using Passport.Application.Result;
using Passport.Application.Validation;
using System.Threading;
using System.Threading.Tasks;

namespace Passport.Application.Command.Authentication.ByCredential
{
    internal sealed class AuthenticationTokenByCredentialValidation : IValidation<AuthenticationTokenByCredentialCommand>
    {
        private readonly IPassportValidation srvValidation;

        public AuthenticationTokenByCredentialValidation(IPassportValidation srvValidation)
        {
            this.srvValidation = srvValidation;
        }

        async ValueTask<IMessageResult<bool>> IValidation<AuthenticationTokenByCredentialCommand>.ValidateAsync(AuthenticationTokenByCredentialCommand mdtCommand, CancellationToken tknCancellation)
        {
            if (tknCancellation.IsCancellationRequested)
                return new MessageResult<bool>(DefaultMessageError.TaskAborted);

            srvValidation.ValidateProvider(mdtCommand.Credential.Provider, "Provider");
            srvValidation.ValidateCredential(mdtCommand.Credential.Credential, "Credential");
            srvValidation.ValidateSignature(mdtCommand.Credential.Signature, "Signature");

            return await Task.FromResult(
                srvValidation.Match(
                    msgError => new MessageResult<bool>(msgError),
                    bResult => new MessageResult<bool>(true))
                );
        }
    }
}