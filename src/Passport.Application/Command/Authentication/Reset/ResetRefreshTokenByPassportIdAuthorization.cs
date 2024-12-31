using Passport.Abstraction.Authorization;
using Passport.Abstraction.Result;
using Passport.Application.Default;
using Passport.Application.Extension;
using Passport.Application.Interface;
using Passport.Application.Result;
using Passport.Application.Transfer;

namespace Passport.Application.Command.Authentication.Reset
{
    internal sealed class ResetRefreshTokenByPassportIdAuthorization : IAuthorization<ResetRefreshTokenByPassportIdCommand>
    {
        public string PassportVisaName { get; } = DefaultPassportVisa.Name.Passport;
        public int PassportVisaLevel { get; } = DefaultPassportVisa.Level.Update;

        private readonly IPassportRepository repoPassport;

        public ResetRefreshTokenByPassportIdAuthorization(IPassportRepository repoPassport)
        {
            this.repoPassport = repoPassport;
        }

        async ValueTask<IMessageResult<bool>> IAuthorization<ResetRefreshTokenByPassportIdCommand>.AuthorizeAsync(ResetRefreshTokenByPassportIdCommand msgMessage, CancellationToken tknCancellation)
        {
            if (tknCancellation.IsCancellationRequested)
                return new MessageResult<bool>(DefaultMessageError.TaskAborted);

            RepositoryResult<PassportTransferObject> rsltPassport = await repoPassport.FindByIdAsync(msgMessage.RestrictedPassportId, tknCancellation);

            return await rsltPassport.MatchAsync(
                msgError => new MessageResult<bool>(new MessageError() { Code = msgError.Code, Description = msgError.Description }),
                async dtoPassport =>
                {
                    Domain.Aggregate.Passport? ppPassport = dtoPassport.Initialize();

                    if (ppPassport is null)
                        return new MessageResult<bool>(DomainError.InitializationHasFailed);

                    if (ppPassport.IsEnabled == false)
                        return new MessageResult<bool>(AuthorizationError.Passport.IsDisabled);

                    return new MessageResult<bool>(true);
                });
        }
    }
}