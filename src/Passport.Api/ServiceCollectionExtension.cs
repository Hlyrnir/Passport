﻿using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Passport.Abstraction.Authentication;
using Passport.Application;
using Passport.Application.Interface;
using Passport.Infrastructure;
using System;

namespace Passport.Api
{
    public static class ServiceCollectionExtension
    {
        public static PassportServiceCollectionBuilder AddPassport<T>(this IServiceCollection cltService, Func<IServiceProvider, IAuthenticationTokenHandler<T>>? dlgAuthentication = null)
        {
            cltService.TryAddScoped<IPassportSetting, PassportSetting>();

            cltService.AddApplicationServiceCollection();
            cltService.AddInfrastructureServiceCollection();

            cltService.TryAddScoped<IAuthenticationTokenHandler<T>>(prvService =>
            {
                IAuthenticationTokenHandler<T>? authTokenHandler = null;

                authTokenHandler = dlgAuthentication?.Invoke(prvService);

                if (authTokenHandler is null)
                    return new DefaultAuthenticationTokenHandler<T>();

                return authTokenHandler;
            });

            return new PassportServiceCollectionBuilder(cltService);
        }
    }
}