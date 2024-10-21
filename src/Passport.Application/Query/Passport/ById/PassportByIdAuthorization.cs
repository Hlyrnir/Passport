using Passport.Abstraction.Authorization;
using Passport.Abstraction.Result;
using Passport.Application.Default;
using Passport.Application.Result;

namespace Passport.Application.Query.Passport.ById
{
    internal sealed class PassportByIdAuthorization : IAuthorization<PassportByIdQuery>
    {
        public string PassportVisaName { get; } = DefaultPassportVisa.Name.Passport;
        public int PassportVisaLevel { get; } = DefaultPassportVisa.Level.Read;

        async ValueTask<IMessageResult<bool>> IAuthorization<PassportByIdQuery>.AuthorizeAsync(PassportByIdQuery msgMessage, CancellationToken tknCancellation)
        {
            if (tknCancellation.IsCancellationRequested)
                return new MessageResult<bool>(DefaultMessageError.TaskAborted);

            return new MessageResult<bool>(true);
        }
    }
}