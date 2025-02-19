using Passport.Abstraction.Authorization;
using Passport.Abstraction.Result;
using Passport.Application.Default;
using Passport.Application.Result;
using System.Threading;
using System.Threading.Tasks;

namespace Passport.Application.Command.PassportHolder.ConfirmPhoneNumber
{
    internal sealed class ConfirmPhoneNumberAuthorization : IAuthorization<ConfirmPhoneNumberCommand>
    {
        public string PassportVisaName { get; } = DefaultPassportVisa.Name.Passport;
        public int PassportVisaLevel { get; } = DefaultPassportVisa.Level.Update;

        async ValueTask<IMessageResult<bool>> IAuthorization<ConfirmPhoneNumberCommand>.AuthorizeAsync(ConfirmPhoneNumberCommand msgMessage, CancellationToken tknCancellation)
        {
            if (tknCancellation.IsCancellationRequested)
                return new MessageResult<bool>(DefaultMessageError.TaskAborted);

            return new MessageResult<bool>(true);
        }
    }
}
