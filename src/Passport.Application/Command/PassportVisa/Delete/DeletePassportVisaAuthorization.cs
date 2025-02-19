using Passport.Abstraction.Authorization;
using Passport.Abstraction.Result;
using Passport.Application.Default;
using Passport.Application.Result;
using System.Threading;
using System.Threading.Tasks;

namespace Passport.Application.Command.PassportVisa.Delete
{
    internal sealed class DeletePassportVisaAuthorization : IAuthorization<DeletePassportVisaCommand>
    {
        public string PassportVisaName { get; } = DefaultPassportVisa.Name.Passport;
        public int PassportVisaLevel { get; } = DefaultPassportVisa.Level.Delete;

        async ValueTask<IMessageResult<bool>> IAuthorization<DeletePassportVisaCommand>.AuthorizeAsync(DeletePassportVisaCommand msgMessage, CancellationToken tknCancellation)
        {
            if (tknCancellation.IsCancellationRequested)
                return new MessageResult<bool>(DefaultMessageError.TaskAborted);

            return new MessageResult<bool>(true);
        }
    }
}
