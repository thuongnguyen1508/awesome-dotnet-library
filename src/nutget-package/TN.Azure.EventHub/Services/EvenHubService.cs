using Azure.Identity;
using Azure.Messaging.EventHubs;
using Azure.Messaging.EventHubs.Producer;
using Microsoft.Extensions.Options;
using Newtonsoft.Json;
using System.Collections.Concurrent;
using TN.Azure.EventHub.Services.Abstractions;

namespace TN.Azure.EventHub.Services
{
    public class EvenHubService : IEvenHubService
    {
        private ConcurrentDictionary<string, EventHubProducerClient> _clients = new ConcurrentDictionary<string, EventHubProducerClient>();
        private EventHubOptions _options { get; set; }

        public EvenHubService(IOptionsMonitor<EventHubOptions> options)
        {
            _options = options.CurrentValue;
        }
        public async Task SendAsync<T>(T item, string eventHubName, Dictionary<string, object>? eventProperties = null)
        {
            EventHubProducerClient client = GetClient(eventHubName);
            var eventText = JsonConvert.SerializeObject(item, Newtonsoft.Json.Formatting.Indented);
            var eventData = new EventData(eventText);
            if (eventProperties != null)
            {
                foreach (var property in eventProperties)
                {
                    eventData.Properties.Add(property.Key, property.Value);
                }
            }

            await client.SendAsync(new List<EventData> { eventData });
        }

        private EventHubProducerClient GetClient(string eventHubName)
        {
            if (!_clients.ContainsKey(eventHubName))
            {
                _clients.TryAdd(eventHubName, _options.UseAzureIdentity ? new EventHubProducerClient(_options.EndPoint, eventHubName, new DefaultAzureCredential()) : new EventHubProducerClient(_options.ConnectionString, eventHubName));
            }
            return _clients[eventHubName];
        }
    }
}
