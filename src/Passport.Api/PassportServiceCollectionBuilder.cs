﻿using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Passport.Application;
using Passport.Application.Interface;
using Passport.Infrastructure;

namespace Passport.Api
{
    public class PassportServiceCollectionBuilder
    {
        private readonly IServiceCollection cltService;

        public virtual IServiceCollection Services { get => cltService; }

        public PassportServiceCollectionBuilder(IServiceCollection cltService)
        {
            this.cltService = cltService;
        }

        public PassportServiceCollectionBuilder AddSqliteDatabase(string sConnectionStringName)
        {
            cltService.AddSqliteDatabase(sConnectionStringName);

            return new PassportServiceCollectionBuilder(cltService);
        }

        public PassportServiceCollectionBuilder Configure(Action<PassportSetting> actnPassportSetting)
        {
            cltService.TryAddScoped<IPassportSetting>(prvService =>
            {
                IPassportSetting ppSetting = prvService.GetRequiredService<IPassportSetting>();

                if (ppSetting is PassportSetting == false)
                    throw new InvalidCastException("IPassportSetting could not be configured.");

                actnPassportSetting((PassportSetting)ppSetting);

                return ppSetting;
            });

            return new PassportServiceCollectionBuilder(cltService);
        }
    }
}