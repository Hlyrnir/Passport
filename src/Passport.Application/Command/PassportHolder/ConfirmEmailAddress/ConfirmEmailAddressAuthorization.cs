using Passport.Abstraction.Authorization;
using Passport.Abstraction.Result;
using Passport.Application.Default;
using Passport.Application.Result;

namespace Passport.Application.Command.PassportHolder.ConfirmEmailAddress
{
    internal sealed class ConfirmEmailAddressAuthorization : IAuthorization<ConfirmEmailAddressCommand>
    {
        public string PassportVisaName { get; } = DefaultPassportVisa.Name.Passport;
        public int PassportVisaLevel { get; } = DefaultPassportVisa.Level.Update;

        async ValueTask<IMessageResult<bool>> IAuthorization<ConfirmEmailAddressCommand>.AuthorizeAsync(ConfirmEmailAddressCommand msgMessage, CancellationToken tknCancellation)
        {
            if (tknCancellation.IsCancellationRequested)
                return new MessageResult<bool>(DefaultMessageError.TaskAborted);

            return await Task.FromResult(new MessageResult<bool>(true));
        }
    }
}
