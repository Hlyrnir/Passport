using Passport.Abstraction.Authorization;
using Passport.Abstraction.Result;
using Passport.Application.Default;
using Passport.Application.Result;
using System.Threading;
using System.Threading.Tasks;

namespace Passport.Application.Command.PassportToken.Delete
{
    internal sealed class DeletePassportTokenAuthorization : IAuthorization<DeletePassportTokenCommand>
    {
        public string PassportVisaName { get; } = DefaultPassportVisa.Name.Passport;
        public int PassportVisaLevel { get; } = DefaultPassportVisa.Level.Delete;

        async ValueTask<IMessageResult<bool>> IAuthorization<DeletePassportTokenCommand>.AuthorizeAsync(DeletePassportTokenCommand msgMessage, CancellationToken tknCancellation)
        {
            if (tknCancellation.IsCancellationRequested)
                return new MessageResult<bool>(DefaultMessageError.TaskAborted);

            return new MessageResult<bool>(true);
        }
    }
}