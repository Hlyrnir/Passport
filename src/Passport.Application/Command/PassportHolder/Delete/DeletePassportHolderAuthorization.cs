using Passport.Abstraction.Authorization;
using Passport.Abstraction.Result;
using Passport.Application.Default;
using Passport.Application.Result;
using System.Threading;
using System.Threading.Tasks;

namespace Passport.Application.Command.PassportHolder.Delete
{
    internal sealed class DeletePassportHolderAuthorization : IAuthorization<DeletePassportHolderCommand>
    {
        public string PassportVisaName { get; } = DefaultPassportVisa.Name.Passport;
        public int PassportVisaLevel { get; } = DefaultPassportVisa.Level.Delete;

        async ValueTask<IMessageResult<bool>> IAuthorization<DeletePassportHolderCommand>.AuthorizeAsync(DeletePassportHolderCommand msgMessage, CancellationToken tknCancellation)
        {
            if (tknCancellation.IsCancellationRequested)
                return new MessageResult<bool>(DefaultMessageError.TaskAborted);

            return new MessageResult<bool>(true);
        }
    }
}