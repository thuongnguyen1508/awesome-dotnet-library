using System.Threading.Tasks;
using TN.EventBus.Events;

namespace TN.EventBus
{
    /// <summary>
    /// Contract for the event bus. The event bus uses a message broker to send and subscribe to events.
    /// </summary>
    public interface IEventBus
    {
        Task PublishAsync<TEvent>(TEvent @event)
            where TEvent : Event;

        void Subscribe<TEvent, TEventHandler>()
            where TEvent : Event
            where TEventHandler : IEventHandler<TEvent>;

        void Unsubscribe<TEvent, TEventHandler>()
            where TEvent : Event
            where TEventHandler : IEventHandler<TEvent>;
    }
}
