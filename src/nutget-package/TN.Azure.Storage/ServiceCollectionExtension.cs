using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using TN.Azure.Storage.Services;
using TN.Azure.Storage.Services.Abtractions;

namespace TN.Azure.Storage
{
    public static class ServiceCollectionExtension
    {
        public static IServiceCollection AddAzureStorageService(this IServiceCollection services, IConfiguration configuration)
        {
            services.Configure<AzureStorageOptions>(options => configuration.GetSection(nameof(AzureStorageOptions)).Bind(options));
            services.AddSingleton<IAzureStorageService, AzureStorageService>();
            return services;
        }
    }
}
