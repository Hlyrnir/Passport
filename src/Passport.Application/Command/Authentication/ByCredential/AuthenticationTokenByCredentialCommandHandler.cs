using Mediator;
using Passport.Abstraction.Authentication;
using Passport.Abstraction.Result;
using Passport.Application.Default;
using Passport.Application.Extension;
using Passport.Application.Interface;
using Passport.Application.Result;
using Passport.Application.Transfer;

namespace Passport.Application.Command.Authentication.ByCredential
{
    public sealed class AuthenticationTokenByCredentialCommandHandler : ICommandHandler<AuthenticationTokenByCredentialCommand, IMessageResult<AuthenticationTokenTransferObject>>
    {
        private readonly TimeProvider prvTime;

        private readonly IPassportRepository repoPassport;
        private readonly IPassportTokenRepository repoToken;

        private readonly IAuthenticationTokenHandler<Guid> authTokenHandler;

        public AuthenticationTokenByCredentialCommandHandler(
            TimeProvider prvTime,
            IPassportRepository repoPassport,
            IPassportTokenRepository repoToken,
            IAuthenticationTokenHandler<Guid> authTokenHandler)
        {
            this.prvTime = prvTime;

            this.repoPassport = repoPassport;
            this.repoToken = repoToken;

            this.authTokenHandler = authTokenHandler;
        }

        public async ValueTask<IMessageResult<AuthenticationTokenTransferObject>> Handle(AuthenticationTokenByCredentialCommand cmdToken, CancellationToken tknCancellation)
        {
            if (tknCancellation.IsCancellationRequested)
                return new MessageResult<AuthenticationTokenTransferObject>(DefaultMessageError.TaskAborted);

            RepositoryResult<int> rsltRemainingAttempt = await repoToken.VerifyCredentialAsync(cmdToken.Credential, prvTime.GetUtcNow(), tknCancellation);

            return await rsltRemainingAttempt.MatchAsync(
                msgError => new MessageResult<AuthenticationTokenTransferObject>(AuthenticationError.CredentialNotFound),
                async iRemainingAttempt =>
                {
                    if (iRemainingAttempt <= 0)
                        return new MessageResult<AuthenticationTokenTransferObject>(AuthenticationError.TooManyAttempts);

                    RepositoryResult<PassportTokenTransferObject> rsltToken = await repoToken.FindTokenByCredentialAsync(cmdToken.Credential, prvTime.GetUtcNow(), tknCancellation);

                    return await rsltToken.MatchAsync(
                        msgError => new MessageResult<AuthenticationTokenTransferObject>(new MessageError() { Code = msgError.Code, Description = msgError.Description }),
                        async dtoPassportToken =>
                        {
                            Domain.Aggregate.PassportToken? ppToken = dtoPassportToken.Initialize();

                            if (ppToken is null)
                                return new MessageResult<AuthenticationTokenTransferObject>(DomainError.InitializationHasFailed);

                            RepositoryResult<PassportTransferObject> rsltPassport = await repoPassport.FindByIdAsync(ppToken.PassportId, tknCancellation);

                            return rsltPassport.Match(
                                msgError => new MessageResult<AuthenticationTokenTransferObject>(new MessageError() { Code = msgError.Code, Description = msgError.Description }),
                                dtoPassport =>
                                {
                                    Domain.Aggregate.Passport? ppPassport = dtoPassport.Initialize();

                                    if (ppPassport is null)
                                        return new MessageResult<AuthenticationTokenTransferObject>(DomainError.InitializationHasFailed);

                                    if (ppPassport.IsExpired(prvTime.GetUtcNow()) == true)
                                        return new MessageResult<AuthenticationTokenTransferObject>(AuthorizationError.Passport.IsExpired);

                                    if (ppPassport.IsEnabled == false)
                                        return new MessageResult<AuthenticationTokenTransferObject>(AuthorizationError.Passport.IsDisabled);

                                    return new AuthenticationTokenTransferObject()
                                    {
                                        ExpiredAt = ppToken.ExpiredAt,
                                        Provider = ppToken.Provider,
                                        RefreshToken = ppToken.RefreshToken,
                                        Token = authTokenHandler.Generate(ppPassport.Id, prvTime)
                                    };
                                });
                        });
                });
        }
    }
}