using Azure;
using Microsoft.Extensions.Azure;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using TN.Azure.EventGrid.Constants;
using TN.Azure.EventGrid.Services;
using TN.Azure.EventGrid.Services.Abstractions;

namespace TN.Azure.EventGrid
{
    public static class ServiceCollectionExtension
    {
        public static IServiceCollection AddEventGridService(this IServiceCollection services, IConfiguration configuration)
        {
            EventGridOption eventGridOption = new();
            configuration.GetSection(nameof(EventGridOption)).Bind(eventGridOption);

            services.AddAzureClients(clientBuilder =>
            {
                // Register Event Grid Client A
                clientBuilder
                    .AddEventGridPublisherClient(
                        new Uri(eventGridOption.Endpoint),
                        new AzureKeyCredential(eventGridOption.Key))
                    .WithName(SystemConstant.EventGridA);

                // Register Event Grid Client B
                clientBuilder
                    .AddEventGridPublisherClient(
                        new Uri(eventGridOption.Endpoint),
                        new AzureKeyCredential(eventGridOption.Key))
                    .WithName(SystemConstant.EventGridB);
            });

            services.AddSingleton<IEventGridService, EventGridService>();
            return services;
        }
    }
}
