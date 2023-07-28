using Microsoft.Extensions.Logging;
using System.Threading.Tasks;
using TN.EventBus.Events;
using TN.Example.Application.IntegrationEvents.Events;

namespace TN.Application.IntegrationEvents.EventHandlers
{
    public class MessageSentEventHandler : IEventHandler<MessageSentEvent>
	{
		private readonly ILogger<MessageSentEventHandler> _logger;

		public MessageSentEventHandler(ILogger<MessageSentEventHandler> logger)
		{
			_logger = logger;
		}

		public Task HandleAsync(MessageSentEvent @event)
		{
			_logger.LogInformation($"Hanndle message sent event {@event.ToString()}");
			return Task.CompletedTask;
		}
	}
}
