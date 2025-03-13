using Azure.Messaging.EventGrid;
using Microsoft.Extensions.Azure;
using TN.Azure.EventGrid.Services.Abstractions;

namespace TN.Azure.EventGrid.Services
{
    public class EventGridService : IEventGridService
    {
        private readonly IAzureClientFactory<EventGridPublisherClient> _clientFactory;
        public EventGridService(IAzureClientFactory<EventGridPublisherClient> clientFactory)
        {
            _clientFactory = clientFactory;
        }
        public async Task SendAsync<T>(T data, string publisherClientName, string subject, string eventType, string dataVersion)
        {
            EventGridEvent eventGridEvent = new(
                subject: subject,
                eventType: eventType,
                dataVersion: dataVersion,
                data: data);

            var client = _clientFactory.CreateClient(publisherClientName);
            await client.SendEventAsync(eventGridEvent);
        }
    }
}
