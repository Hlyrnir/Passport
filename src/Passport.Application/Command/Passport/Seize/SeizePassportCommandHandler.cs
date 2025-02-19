using Mediator;
using Passport.Abstraction.Result;
using Passport.Application.Default;
using Passport.Application.Interface;
using Passport.Application.Result;
using Passport.Application.Transfer;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace Passport.Application.Command.Passport.Seize
{
    public sealed class SeizePassportCommandHandler : ICommandHandler<SeizePassportCommand, IMessageResult<bool>>
    {
        private readonly TimeProvider prvTime;

        private readonly IPassportRepository repoPassport;

        public SeizePassportCommandHandler(
            TimeProvider prvTime,
            IPassportRepository repoPassport)
        {
            this.prvTime = prvTime;
            this.repoPassport = repoPassport;
        }

        public async ValueTask<IMessageResult<bool>> Handle(SeizePassportCommand msgMessage, CancellationToken tknCancellation)
        {
            if (tknCancellation.IsCancellationRequested)
                return new MessageResult<bool>(DefaultMessageError.TaskAborted);

            RepositoryResult<PassportTransferObject> rsltPassport = await repoPassport.FindByIdAsync(msgMessage.PassportIdToSeize, tknCancellation);

            return await rsltPassport.MatchAsync(
                msgError => new MessageResult<bool>(new MessageError() { Code = msgError.Code, Description = msgError.Description }),
                async dtoPassport =>
                {
                    RepositoryResult<bool> rsltDelete = await repoPassport.DeleteAsync(dtoPassport, tknCancellation);

                    return rsltDelete.Match(
                        msgError => new MessageResult<bool>(new MessageError() { Code = msgError.Code, Description = msgError.Description }),
                        bResult => new MessageResult<bool>(bResult));
                });
        }
    }
}