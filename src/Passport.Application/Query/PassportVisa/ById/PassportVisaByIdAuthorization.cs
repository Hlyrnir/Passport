using Passport.Abstraction.Authorization;
using Passport.Abstraction.Result;
using Passport.Application.Default;
using Passport.Application.Result;

namespace Passport.Application.Query.PassportVisa.ById
{
    internal sealed class PassportVisaByIdAuthorization : IAuthorization<PassportVisaByIdQuery>
    {
        public string PassportVisaName { get; } = DefaultPassportVisa.Name.Passport;
        public int PassportVisaLevel { get; } = DefaultPassportVisa.Level.Read;

        async ValueTask<IMessageResult<bool>> IAuthorization<PassportVisaByIdQuery>.AuthorizeAsync(PassportVisaByIdQuery msgMessage, CancellationToken tknCancellation)
        {
            if (tknCancellation.IsCancellationRequested)
                return new MessageResult<bool>(DefaultMessageError.TaskAborted);

            return new MessageResult<bool>(true);
        }
    }
}