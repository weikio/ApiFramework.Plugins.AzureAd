using Microsoft.Extensions.DependencyInjection;
using Weikio.ApiFramework.Abstractions.DependencyInjection;
using Weikio.ApiFramework.SDK;

namespace Weikio.ApiFramework.Plugins.AzureAD
{
    public static class ServiceExtensions
    {
        public static IApiFrameworkBuilder AddAzureAdApi(this IApiFrameworkBuilder builder, string endpoint = null, AzureAdOptions configuration = null)
        {
            builder.Services.AddAzureAdApi(endpoint, configuration);

            return builder;
        }

        public static IServiceCollection AddAzureAdApi(this IServiceCollection services, string endpoint = null, AzureAdOptions configuration = null)
        {
            services.RegisterPlugin(endpoint, configuration);

            return services;
        }
    }
}
