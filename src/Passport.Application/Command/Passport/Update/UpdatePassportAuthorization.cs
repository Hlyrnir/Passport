using Passport.Abstraction.Authorization;
using Passport.Abstraction.Result;
using Passport.Application.Default;
using Passport.Application.Extension;
using Passport.Application.Interface;
using Passport.Application.Result;
using Passport.Application.Transfer;

namespace Passport.Application.Command.Passport.Update
{
    internal sealed class UpdatePassportAuthorization : IAuthorization<UpdatePassportCommand>
    {
        public string PassportVisaName { get; } = DefaultPassportVisa.Name.Passport;
        public int PassportVisaLevel { get; } = DefaultPassportVisa.Level.Update;

        private readonly IPassportRepository repoPassport;

        public UpdatePassportAuthorization(IPassportRepository repoPassport)
        {
            this.repoPassport = repoPassport;
        }

        async ValueTask<IMessageResult<bool>> IAuthorization<UpdatePassportCommand>.AuthorizeAsync(UpdatePassportCommand msgMessage, CancellationToken tknCancellation)
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

                    if (ppPassport.IsAuthority == false)
                        return new MessageResult<bool>(AuthorizationError.Passport.NotAuthorized);

                    return new MessageResult<bool>(true);
                });
        }
    }
}