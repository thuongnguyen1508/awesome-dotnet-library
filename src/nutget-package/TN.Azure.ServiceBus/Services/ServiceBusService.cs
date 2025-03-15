using Azure.Identity;
using Azure.Messaging.ServiceBus;
using Microsoft.Extensions.Options;
using Newtonsoft.Json;
using TN.Azure.ServiceBus.Services.Abstractions;

namespace TN.Azure.ServiceBus.Services
{
    public class ServiceBusService : IServiceBusService
    {
        private ServiceBusClient _serviceBusClient { get; set; }

        private readonly ServicebusOptions _options;

        public ServiceBusService(IOptionsMonitor<ServicebusOptions> options)
        {
            _options = options.CurrentValue;
            _serviceBusClient = _options.UseAzureIdentity ? new ServiceBusClient(_options.EndPoint, new DefaultAzureCredential()) : new ServiceBusClient(_options.ConnectionString);
        }

        public async Task SendAsync<T>(T item, string topic, Dictionary<string, object>? eventProperties = null)
        {
            var serviceBusSender = _serviceBusClient.CreateSender(topic);
            string message = JsonConvert.SerializeObject(item);
            ServiceBusMessage serviceBusMessage = new ServiceBusMessage(message);
            if (eventProperties != null)
            {
                foreach (var property in eventProperties)
                {
                    serviceBusMessage.ApplicationProperties.Add(property.Key, property.Value);
                }
            }
            await serviceBusSender.SendMessageAsync(serviceBusMessage);
        }
    }
}
