using Asp.Versioning;
using Asp.Versioning.Builder;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Routing;

namespace Passport.Api.Endpoint
{
    public static class EndpointVersion
    {
        public static ApiVersionSet? VersionSet { get; private set; }

        public static IEndpointRouteBuilder AddEndpointVersionSet(this IEndpointRouteBuilder epBuilder)
        {
            VersionSet = epBuilder.NewApiVersionSet()
                .HasApiVersion(new ApiVersion(1.0))
                .ReportApiVersions()
                .Build();

            return epBuilder;
        }
    }
}
