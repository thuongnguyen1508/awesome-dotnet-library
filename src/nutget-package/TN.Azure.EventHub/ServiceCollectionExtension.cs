using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using TN.Azure.EventHub.Services;
using TN.Azure.EventHub.Services.Abstractions;

namespace TN.Azure.EventHub
{
    public static class ServiceCollectionExtension
    {
        public static IServiceCollection AddEventHubService(this IServiceCollection services, IConfiguration configuration)
        {
            services.Configure<EventHubOptions>(options => configuration.GetSection(nameof(EventHubOptions)).Bind(options));
            services.AddSingleton<IEvenHubService, EvenHubService>();
            return services;
        }
    }
}
