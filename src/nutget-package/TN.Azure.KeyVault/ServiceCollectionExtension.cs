using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using TN.Azure.KeyVault.Services;
using TN.Azure.KeyVault.Services.Abstractions;

namespace TN.Azure.KeyVault
{
    public static class ServiceCollectionExtension
    {
        public static IServiceCollection AddKeyVault(this IServiceCollection services, IConfiguration configuration)
        {
            services.Configure<KeyVaultOptions>(options => configuration.GetSection(nameof(KeyVaultOptions)).Bind(options));
            services.AddSingleton<IKeyVaultService, KeyVaultService>();
            services.AddSingleton<ICryptographyService, CryptographyService>();
            return services;
        }
    }
}
