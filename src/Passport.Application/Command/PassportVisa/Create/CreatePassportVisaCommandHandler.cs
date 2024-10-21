using Mediator;
using Passport.Application.Default;
using Passport.Application.Extension;
using Passport.Application.Interface;
using Passport.Application.Result;

namespace Passport.Application.Command.PassportVisa.Create
{
    public sealed class CreatePassportVisaCommandHandler : ICommandHandler<CreatePassportVisaCommand, MessageResult<Guid>>
    {
        private readonly TimeProvider prvTime;
        private readonly IPassportVisaRepository repoVisa;

        public CreatePassportVisaCommandHandler(
            TimeProvider prvTime,
            IPassportVisaRepository repoVisa)
        {
            this.prvTime = prvTime;
            this.repoVisa = repoVisa;
        }

        public async ValueTask<MessageResult<Guid>> Handle(CreatePassportVisaCommand msgMessage, CancellationToken tknCancellation)
        {
            if (tknCancellation.IsCancellationRequested)
                return new MessageResult<Guid>(DefaultMessageError.TaskAborted);

            Domain.Aggregate.PassportVisa? ppVisa = Domain.Aggregate.PassportVisa.Create(
                sName: msgMessage.Name,
                iLevel: msgMessage.Level);

            if (ppVisa is null)
                return new MessageResult<Guid>(new MessageError() { Code = DomainError.Code.Method, Description = "Visa has not been created." });

            RepositoryResult<bool> rsltVisa = await repoVisa.InsertAsync(ppVisa.MapToTransferObject(), prvTime.GetUtcNow(), tknCancellation);

            return rsltVisa.Match(
                msgError => new MessageResult<Guid>(new MessageError() { Code = msgError.Code, Description = msgError.Description }),
                bResult => new MessageResult<Guid>(ppVisa.Id));
        }
    }
}