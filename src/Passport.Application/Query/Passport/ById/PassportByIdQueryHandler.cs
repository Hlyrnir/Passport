using Mediator;
using Passport.Application.Default;
using Passport.Application.Interface;
using Passport.Application.Result;
using Passport.Application.Transfer;

namespace Passport.Application.Query.Passport.ById
{
    public sealed class PassportByIdQueryHandler : IQueryHandler<PassportByIdQuery, MessageResult<PassportByIdResult>>
    {
        private readonly IPassportRepository repoPassport;
        private readonly IPassportVisaRepository repoVisa;

        public PassportByIdQueryHandler(IPassportRepository repoPassport, IPassportVisaRepository repoVisa)
        {
            this.repoPassport = repoPassport;
            this.repoVisa = repoVisa;
        }

        public async ValueTask<MessageResult<PassportByIdResult>> Handle(PassportByIdQuery qryQuery, CancellationToken tknCancellation)
        {
            if (tknCancellation.IsCancellationRequested)
                return new MessageResult<PassportByIdResult>(DefaultMessageError.TaskAborted);

            RepositoryResult<PassportTransferObject> rsltPassport = await repoPassport.FindByIdAsync(qryQuery.PassportId, tknCancellation);

            return rsltPassport.Match(
                msgError => new MessageResult<PassportByIdResult>(new MessageError() { Code = msgError.Code, Description = msgError.Description }),
                 dtoPassport =>
                 {
                     PassportByIdResult qryResult = new PassportByIdResult()
                     {
                         Passport = dtoPassport
                     };

                     return new MessageResult<PassportByIdResult>(qryResult);
                 });
        }
    }
}