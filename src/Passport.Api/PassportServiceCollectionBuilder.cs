using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Passport.Application;
using Passport.Application.Interface;
using Passport.Infrastructure;
using System;

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

        public PassportServiceCollectionBuilder Configure(Action<PassportSetting> actnPassportSetting)
        {
            cltService.Replace(new ServiceDescriptor(
                typeof(IPassportSetting),
                prvService =>
                {
                    PassportSetting ppSetting = new PassportSetting();

                    actnPassportSetting(ppSetting);

                    return ppSetting;
                },
                ServiceLifetime.Scoped));

            return new PassportServiceCollectionBuilder(cltService);
        }

        public PassportServiceCollectionBuilder AddSqliteDatabase(string sConnectionStringName)
        {
            cltService.AddSqliteDatabase(sConnectionStringName);

            return new PassportServiceCollectionBuilder(cltService);
        }
    }
}