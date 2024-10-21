using Passport.Abstraction.Authorization;
using Passport.Abstraction.Result;
using Passport.Application.Default;
using Passport.Application.Result;

namespace Passport.Application.Command.PassportVisa.Update
{
    internal sealed class UpdatePassportVisaAuthorization : IAuthorization<UpdatePassportVisaCommand>
    {
        public string PassportVisaName { get; } = DefaultPassportVisa.Name.Passport;
        public int PassportVisaLevel { get; } = DefaultPassportVisa.Level.Update;

        async ValueTask<IMessageResult<bool>> IAuthorization<UpdatePassportVisaCommand>.AuthorizeAsync(UpdatePassportVisaCommand msgMessage, CancellationToken tknCancellation)
        {
            if (tknCancellation.IsCancellationRequested)
                return new MessageResult<bool>(DefaultMessageError.TaskAborted);

            return new MessageResult<bool>(true);
        }
    }
}
