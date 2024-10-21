using Passport.Abstraction.Result;
using Passport.Abstraction.Validation;
using Passport.Application.Default;
using Passport.Application.Result;
using Passport.Application.Validation;

namespace Passport.Application.Command.Authentication.ByRefreshToken
{
    internal sealed class AuthenticationTokenByRefreshTokenValidation : IValidation<AuthenticationTokenByRefreshTokenCommand>
    {
        private readonly IPassportValidation srvValidation;

        public AuthenticationTokenByRefreshTokenValidation(IPassportValidation srvValidation)
        {
            this.srvValidation = srvValidation;
        }

        async ValueTask<IMessageResult<bool>> IValidation<AuthenticationTokenByRefreshTokenCommand>.ValidateAsync(AuthenticationTokenByRefreshTokenCommand msgMessage, CancellationToken tknCancellation)
        {
            if (tknCancellation.IsCancellationRequested)
                return new MessageResult<bool>(DefaultMessageError.TaskAborted);

            srvValidation.ValidateGuid(msgMessage.PassportId, "Id");
            srvValidation.ValidateProvider(msgMessage.Provider, "Provider");

            if (string.IsNullOrWhiteSpace(msgMessage.RefreshToken) == true)
                srvValidation.Add(ValidationError.Authentication.InvalidRefreshToken);

            return await Task.FromResult(
                srvValidation.Match(
                    msgError => new MessageResult<bool>(msgError),
                    bResult => new MessageResult<bool>(true))
                );
        }
    }
}