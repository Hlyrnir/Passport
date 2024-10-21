using Microsoft.Extensions.DependencyInjection;

namespace Passport.Api
{
    public class PassportServiceCollectionBuilder
    {

        public virtual IServiceCollection Services { get; }

        public PassportServiceCollectionBuilder(IServiceCollection clctService)
        {
            Services = clctService;
        }
    }
}