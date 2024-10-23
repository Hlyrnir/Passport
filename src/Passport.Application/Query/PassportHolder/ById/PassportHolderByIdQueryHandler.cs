using Mediator;
using Passport.Application.Default;
using Passport.Application.Interface;
using Passport.Application.Result;
using Passport.Application.Transfer;

namespace Passport.Application.Query.PassportHolder.ById
{
    public sealed class PassportHolderByIdQueryHandler : IQueryHandler<PassportHolderByIdQuery, MessageResult<PassportHolderByIdResult>>
    {
        private readonly IPassportSetting ppSetting;
        private readonly IPassportHolderRepository repoHolder;

        public PassportHolderByIdQueryHandler(IPassportSetting ppSetting, IPassportHolderRepository repoHolder)
        {
            this.ppSetting = ppSetting;
            this.repoHolder = repoHolder;
        }

        public async ValueTask<MessageResult<PassportHolderByIdResult>> Handle(PassportHolderByIdQuery qryQuery, CancellationToken tknCancellation)
        {
            if (tknCancellation.IsCancellationRequested)
                return new MessageResult<PassportHolderByIdResult>(DefaultMessageError.TaskAborted);

            RepositoryResult<PassportHolderTransferObject> rsltPassportHolder = await repoHolder.FindByIdAsync(qryQuery.PassportHolderId, tknCancellation);

            return rsltPassportHolder.Match(
                msgError => new MessageResult<PassportHolderByIdResult>(new MessageError() { Code = msgError.Code, Description = msgError.Description }),
                dtoPassportHolder =>
                {
                    PassportHolderByIdResult qryResult = new PassportHolderByIdResult()
                    {
                        PassportHolder = dtoPassportHolder
                    };

                    return new MessageResult<PassportHolderByIdResult>(qryResult);
                });
        }
    }
}