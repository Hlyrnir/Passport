using Passport.Abstraction.Authorization;
using Passport.Abstraction.Result;
using Passport.Application.Default;
using Passport.Application.Result;

namespace Passport.Application.Command.Passport.Seize
{
    internal sealed class SeizePassportAuthorization : IAuthorization<SeizePassportCommand>
    {
        public string PassportVisaName { get; } = DefaultPassportVisa.Name.Passport;
        public int PassportVisaLevel { get; } = DefaultPassportVisa.Level.Delete;

        async ValueTask<IMessageResult<bool>> IAuthorization<SeizePassportCommand>.AuthorizeAsync(SeizePassportCommand msgMessage, CancellationToken tknCancellation)
        {
            if (tknCancellation.IsCancellationRequested)
                return new MessageResult<bool>(DefaultMessageError.TaskAborted);

            return new MessageResult<bool>(true);
        }
    }
}
