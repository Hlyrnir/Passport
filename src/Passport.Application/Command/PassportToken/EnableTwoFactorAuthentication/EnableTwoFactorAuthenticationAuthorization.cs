﻿using Passport.Abstraction.Authorization;
using Passport.Abstraction.Result;
using Passport.Application.Default;
using Passport.Application.Result;
using System.Threading;
using System.Threading.Tasks;

namespace Passport.Application.Command.PassportToken.EnableTwoFactorAuthentication
{
    internal sealed class EnableTwoFactorAuthenticationAuthorization : IAuthorization<EnableTwoFactorAuthenticationCommand>
    {
        public string PassportVisaName { get; } = DefaultPassportVisa.Name.Passport;
        public int PassportVisaLevel { get; } = DefaultPassportVisa.Level.Update;

        async ValueTask<IMessageResult<bool>> IAuthorization<EnableTwoFactorAuthenticationCommand>.AuthorizeAsync(EnableTwoFactorAuthenticationCommand msgMessage, CancellationToken tknCancellation)
        {
            if (tknCancellation.IsCancellationRequested)
                return new MessageResult<bool>(DefaultMessageError.TaskAborted);

            return new MessageResult<bool>(true);
        }
    }
}
