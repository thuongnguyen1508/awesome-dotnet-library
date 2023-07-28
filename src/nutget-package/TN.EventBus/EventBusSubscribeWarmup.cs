using System;
using System.Threading;
using System.Threading.Tasks;
using TN.EventBus.Events;
using TN.EventBus.Subscriptions;
using TN.Warmups;

namespace TN.EventBus
{
    public class EventBusSubscribeWarmup : IStartupTask
    {
        private readonly IEventBus _eventBus;
        private readonly DefinedSubscriptionMessageType _definedMessageType;
        private readonly IServiceProvider _serviceProvider;

        public EventBusSubscribeWarmup(IEventBus eventBus, DefinedSubscriptionMessageType definedMessageType, IServiceProvider serviceProvider)
        {
            _eventBus = eventBus;
            _definedMessageType = definedMessageType;
            _serviceProvider = serviceProvider;
        }

        public Task ExecuteAsync(CancellationToken cancellationToken = default)
        {
            var eventHandlerType = typeof(IEventHandler<>);

            foreach (var item in _definedMessageType.MessageTypes)
            {
                var subscribeMethod = _eventBus.GetType().GetMethod(nameof(IEventBus.Subscribe));
                var handlerType = eventHandlerType.MakeGenericType(item.Key);

                subscribeMethod.MakeGenericMethod(item.Key, handlerType).Invoke(_eventBus, null);
            }

            return Task.CompletedTask;
        }
    }
}
