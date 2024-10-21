using Mediator;
using Passport.Application.Default;
using Passport.Application.Interface;
using Passport.Application.Result;
using Passport.Application.Transfer;

namespace Passport.Application.Query.PassportVisa.ByPassportId
{
    public sealed class PassportVisaByPassportIdQueryHandler : IQueryHandler<PassportVisaByPassportIdQuery, MessageResult<PassportVisaByPassportIdResult>>
    {
        private readonly IPassportVisaRepository repoVisa;

        public PassportVisaByPassportIdQueryHandler(IPassportVisaRepository repoVisa)
        {
            this.repoVisa = repoVisa;
        }

        public async ValueTask<MessageResult<PassportVisaByPassportIdResult>> Handle(PassportVisaByPassportIdQuery msgMessage, CancellationToken tknCancellation)
        {
            if (tknCancellation.IsCancellationRequested)
                return new MessageResult<PassportVisaByPassportIdResult>(DefaultMessageError.TaskAborted);

            RepositoryResult<IEnumerable<PassportVisaTransferObject>> rsltPassportVisa = await repoVisa.FindByPassportAsync(msgMessage.PassportIdToFind, tknCancellation);

            return rsltPassportVisa.Match(
                msgError => new MessageResult<PassportVisaByPassportIdResult>(new MessageError() { Code = msgError.Code, Description = msgError.Description }),
                enumPassportVisa =>
                {
                    PassportVisaByPassportIdResult qryResult = new PassportVisaByPassportIdResult()
                    {
                        PassportVisa = enumPassportVisa
                    };

                    return new MessageResult<PassportVisaByPassportIdResult>(qryResult);
                });
        }
    }
}