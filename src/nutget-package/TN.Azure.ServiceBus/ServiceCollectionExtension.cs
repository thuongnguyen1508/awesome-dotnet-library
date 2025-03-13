using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using TN.Azure.ServiceBus.Services;
using TN.Azure.ServiceBus.Services.Abstractions;

namespace TN.Azure.ServiceBus
{
    public static class ServiceCollectionExtension
    {
        public static IServiceCollection AddEventHubService(this IServiceCollection services, IConfiguration configuration)
        {
            services.Configure<ServicebusOptions>(options => configuration.GetSection(nameof(ServicebusOptions)).Bind(options));
            services.AddSingleton<IServiceBusService, ServiceBusService>();
            return services;
        }
    }
}
