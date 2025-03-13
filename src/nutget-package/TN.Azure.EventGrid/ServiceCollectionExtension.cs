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
                foreach (var publisherName in eventGridOption.EventGridPublisherNameDefinition)
                {
                    clientBuilder
                        .AddEventGridPublisherClient(
                            new Uri(eventGridOption.Endpoint),
                            new AzureKeyCredential(eventGridOption.Key))
                        .WithName(publisherName);
                }
            });

            services.AddSingleton<IEventGridService, EventGridService>();
            return services;
        }
    }
}
