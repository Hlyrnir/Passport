using Passport.Abstraction.Authorization;
using Passport.Abstraction.Result;
using Passport.Application.Default;
using Passport.Application.Result;

namespace Passport.Application.Query.PassportVisa.ByPassportId
{
    internal sealed class PassportVisaByPassportIdAuthorization : IAuthorization<PassportVisaByPassportIdQuery>
    {
        public string PassportVisaName { get; } = DefaultPassportVisa.Name.Passport;
        public int PassportVisaLevel { get; } = DefaultPassportVisa.Level.Read;

        async ValueTask<IMessageResult<bool>> IAuthorization<PassportVisaByPassportIdQuery>.AuthorizeAsync(PassportVisaByPassportIdQuery msgMessage, CancellationToken tknCancellation)
        {
            if (tknCancellation.IsCancellationRequested)
                return new MessageResult<bool>(DefaultMessageError.TaskAborted);

            return new MessageResult<bool>(true);
        }
    }
}