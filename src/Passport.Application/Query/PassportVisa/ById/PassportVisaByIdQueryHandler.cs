using Mediator;
using Passport.Application.Default;
using Passport.Application.Interface;
using Passport.Application.Result;
using Passport.Application.Transfer;

namespace Passport.Application.Query.PassportVisa.ById
{
    public sealed class PassportVisaByIdQueryHandler : IQueryHandler<PassportVisaByIdQuery, MessageResult<PassportVisaByIdResult>>
    {
        private readonly IPassportVisaRepository repoVisa;

        public PassportVisaByIdQueryHandler(IPassportVisaRepository repoVisa)
        {
            this.repoVisa = repoVisa;
        }

        public async ValueTask<MessageResult<PassportVisaByIdResult>> Handle(PassportVisaByIdQuery msgMessage, CancellationToken tknCancellation)
        {
            if (tknCancellation.IsCancellationRequested)
                return new MessageResult<PassportVisaByIdResult>(DefaultMessageError.TaskAborted);

            RepositoryResult<PassportVisaTransferObject> rsltPassportVisa = await repoVisa.FindByIdAsync(msgMessage.PassportVisaId, tknCancellation);

            return rsltPassportVisa.Match(
                msgError => new MessageResult<PassportVisaByIdResult>(new MessageError() { Code = msgError.Code, Description = msgError.Description }),
                dtoPassportVisa =>
                {
                    PassportVisaByIdResult qryResult = new PassportVisaByIdResult()
                    {
                        PassportVisa = dtoPassportVisa
                    };

                    return new MessageResult<PassportVisaByIdResult>(qryResult);
                });
        }
    }
}