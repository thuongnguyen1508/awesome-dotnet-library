using TN.EventBus.Events;

namespace TN.Example.Application.IntegrationEvents.Events
{
    public class MessageSentEvent : Event
    {
        public string Message { get; set; }

        public override string ToString()
        {
            return $"ID: {Id} - Created at: {CreatedAt:MM/dd/yyyy} - Message: {Message}";
        }
    }
}
