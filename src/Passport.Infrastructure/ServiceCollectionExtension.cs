using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Passport.Application.Default;
using Passport.Application.Interface;
using Passport.Infrastructure.Persistence;

namespace Passport.Infrastructure
{
    public static class ServiceCollectionExtension
    {
        public static IServiceCollection AddInfrastructureServiceCollection(this IServiceCollection cltService)
        {
            cltService.TryAddKeyedScoped<IUnitOfWork, UnitOfWork>(DefaultKeyedServiceName.UnitOfWork);

            cltService.TryAddTransient<IPassportRepository, PassportRepository>();
            cltService.TryAddTransient<IPassportHolderRepository, PassportHolderRepository>();
            cltService.TryAddTransient<IPassportTokenRepository, PassportTokenRepository>();
            cltService.TryAddTransient<IPassportVisaRepository, PassportVisaRepository>();

            return cltService;
        }

        public static IServiceCollection AddSqliteDatabase(this IServiceCollection cltService, string sConnectionStringName)
        {
            cltService.TryAddKeyedScoped<IDataAccess>(DefaultKeyedServiceName.DataAccess, (prvService, sName) =>
            {
                IConfiguration cfgConfiguration = prvService.GetRequiredService<IConfiguration>();

                return new SqliteDataAccess(cfgConfiguration, sConnectionStringName);
            });

            return cltService;
        }
    }
}