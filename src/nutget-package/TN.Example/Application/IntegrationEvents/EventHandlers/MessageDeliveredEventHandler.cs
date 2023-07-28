using Microsoft.Extensions.Logging;
using System.Threading.Tasks;
using TN.EventBus.Events;
using TN.Example.Application.IntegrationEvents.Events;

namespace TN.Example.Application.IntegrationEvents.EventHandlers
{
    public class MessageDeliveredEventHandler : IEventHandler<MessageDeliveredEvent>
	{
		private readonly ILogger<MessageDeliveredEventHandler> _logger;

		public MessageDeliveredEventHandler(ILogger<MessageDeliveredEventHandler> logger)
		{
			_logger = logger;
		}

		public Task HandleAsync(MessageDeliveredEvent @event)
		{
			_logger.LogInformation($"Hanndle message delivered event {@event.ToString()}");
			return Task.CompletedTask;
		}
	}
}
