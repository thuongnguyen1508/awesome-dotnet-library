using Microsoft.Extensions.Logging;
using Polly;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;
using RabbitMQ.Client.Exceptions;
using System;
using System.Collections.Concurrent;
using System.Net.Sockets;
using System.Reflection;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using TN.EventBus.Events;
using TN.EventBus.RabbitMQ.Connection;

namespace TN.EventBus.RabbitMQ.Bus
{
    /// <summary>
    /// Event Bus implementation that uses RabbitMQ as the message broker.
    /// The implementation is based on eShopOnContainers (Microsoft's tutorial about microservices in .NET Core), but it implements some features I have found that are based in different libraries.
    /// 
    /// References:
    /// - https://docs.microsoft.com/en-us/dotnet/architecture/microservices/multi-container-microservice-net-applications/integration-event-based-microservice-communications
    /// - https://docs.microsoft.com/en-us/dotnet/architecture/microservices/multi-container-microservice-net-applications/rabbitmq-event-bus-development-test-environment
    /// - https://github.com/ojdev/RabbitMQ.EventBus.AspNetCore
    /// </summary>
    public class RabbitMQEventBus : IEventBus
    {
        private readonly string _exchangeName;
        private readonly int _publishRetryCount = 5;
        private readonly ConcurrentDictionary<string, string> _queueMappings;
        private readonly TimeSpan _subscribeRetryTime = TimeSpan.FromSeconds(5);

        private readonly IPersistentConnection _persistentConnection;
        private readonly EventBusSubscriptionManager _subscriptionsManager;
        private readonly IServiceProvider _serviceProvider;

        private readonly ILogger<RabbitMQEventBus> _logger;

        private IModel _consumerChannel;

        public RabbitMQEventBus(
            IPersistentConnection persistentConnection,
            EventBusSubscriptionManager subscriptionsManager,
            IServiceProvider serviceProvider,
            ILogger<RabbitMQEventBus> logger,
            RabbitMQSettings rabbitMQSettings)
        {
            _persistentConnection = persistentConnection ?? throw new ArgumentNullException(nameof(persistentConnection));
            _subscriptionsManager = subscriptionsManager ?? throw new ArgumentNullException(nameof(subscriptionsManager));
            _serviceProvider = serviceProvider;
            _logger = logger;
            _exchangeName = rabbitMQSettings.ExchangeName ?? throw new ArgumentNullException(nameof(rabbitMQSettings.ExchangeName));
            _queueMappings = new ConcurrentDictionary<string, string>(rabbitMQSettings.QueueMappings);

            ConfigureMessageBroker();
        }

        public async Task PublishAsync<TEvent>(TEvent @event)
            where TEvent : Event
        {
            if (!_persistentConnection.IsConnected)
            {
                _persistentConnection.TryConnect();
            }

            var policy = Policy
                .Handle<BrokerUnreachableException>()
                .Or<SocketException>()
                .WaitAndRetryAsync(_publishRetryCount, retryAttempt => TimeSpan.FromSeconds(Math.Pow(2, retryAttempt)), (exception, timeSpan) =>
                {
                    _logger.LogWarning(exception, "Could not publish event #{EventId} after {Timeout} seconds: {ExceptionMessage}.", @event.Id, $"{timeSpan.TotalSeconds:n1}", exception.Message);
                });

            var eventName = @event.GetType().Name;
            if (!_queueMappings.TryGetValue(eventName, out var queueName))
            {
                throw new InvalidOperationException($"Queue mapping not found for event '{eventName}'.");
            }

            _logger.LogTrace("Creating RabbitMQ channel to publish event #{EventId} ({EventName})...", @event.Id, eventName);

            using (var channel = _persistentConnection.CreateModel())
            {
                _logger.LogTrace("Declaring RabbitMQ exchange to publish event #{EventId}...", @event.Id);

                channel.ExchangeDeclare(exchange: _exchangeName, type: ExchangeType.Direct);

                var message = JsonSerializer.Serialize(@event);
                var body = Encoding.UTF8.GetBytes(message);

                await policy.ExecuteAsync(async () =>
                {
                    var properties = channel.CreateBasicProperties();
                    properties.DeliveryMode = (byte)DeliveryMode.Persistent;

                    _logger.LogTrace("Publishing event to RabbitMQ with ID #{EventId}...", @event.Id);

                    await Task.Run(() =>
                    {
                        channel.BasicPublish(
                            exchange: _exchangeName,
                            routingKey: eventName,
                            mandatory: true,
                            basicProperties: properties,
                            body: body);
                    });

                    _logger.LogTrace("Published event with ID #{EventId}.", @event.Id);
                });
            }
        }

        public void Subscribe<TEvent, TEventHandler>()
            where TEvent : Event
            where TEventHandler : IEventHandler<TEvent>
        {
            var eventName = _subscriptionsManager.GetEventIdentifier<TEvent>();
            var eventHandlerName = typeof(TEventHandler).Name;
            _queueMappings.TryGetValue(eventName, out var queueName);

            AddQueueBindForEventSubscription(eventName);

            _logger.LogInformation("Subscribing to event {EventName} with {EventHandler}...", eventName, eventHandlerName);

            _subscriptionsManager.AddSubscription<TEvent, TEventHandler>();

            if (!string.IsNullOrWhiteSpace(queueName))
                StartBasicConsume(queueName);

            _logger.LogInformation("Subscribed to event {EventName} with {EvenHandler}.", eventName, eventHandlerName);
        }

        public void Unsubscribe<TEvent, TEventHandler>()
            where TEvent : Event
            where TEventHandler : IEventHandler<TEvent>
        {
            var eventName = _subscriptionsManager.GetEventIdentifier<TEvent>();

            _logger.LogInformation("Unsubscribing from event {EventName}...", eventName);

            _subscriptionsManager.RemoveSubscription<TEvent, TEventHandler>();

            _logger.LogInformation("Unsubscribed from event {EventName}.", eventName);
        }

        private void ConfigureMessageBroker()
        {
            _consumerChannel = CreateConsumerChannel();
            _subscriptionsManager.OnEventRemoved += SubscriptionManager_OnEventRemoved;
            _persistentConnection.OnReconnectedAfterConnectionFailure += PersistentConnection_OnReconnectedAfterConnectionFailure;
        }

        private IModel CreateConsumerChannel()
        {
            if (!_persistentConnection.IsConnected)
            {
                _persistentConnection.TryConnect();
            }

            _logger.LogTrace("Creating RabbitMQ consumer channel...");

            var channel = _persistentConnection.CreateModel();

            channel.ExchangeDeclare(exchange: _exchangeName, type: ExchangeType.Direct);

            var queues = _queueMappings.Values;
            foreach (var queue in queues)
            {
                channel.QueueDeclare
                (
                    queue: queue,
                    durable: true,
                    exclusive: false,
                    autoDelete: false,
                    arguments: null
                );
            }


            channel.CallbackException += (sender, ea) =>
            {
                _logger.LogWarning(ea.Exception, "Recreating RabbitMQ consumer channel...");
                DoCreateConsumerChannel();
            };

            _logger.LogTrace("Created RabbitMQ consumer channel.");


            return channel;
        }

        private void StartBasicConsume()
        {
            _logger.LogTrace("Starting RabbitMQ basic consume...");

            if (_consumerChannel == null)
            {
                _logger.LogError("Could not start basic consume because consumer channel is null.");
                return;
            }

            var consumer = new AsyncEventingBasicConsumer(_consumerChannel);
            consumer.Received += Consumer_Received;

            var queues = _queueMappings.Values;
            foreach (var queue in queues)
            {
                _consumerChannel.BasicConsume
                (
                    queue: queue,
                    autoAck: false,
                    consumer: consumer
                );
            }

            _logger.LogTrace("Started RabbitMQ basic consume.");
        }

        private void StartBasicConsume(string queue)
        {
            _logger.LogTrace($"Starting RabbitMQ basic consume on queue...{queue}");

            if (_consumerChannel == null)
            {
                _logger.LogError("Could not start basic consume because consumer channel is null.");
                return;
            }

            var consumer = new AsyncEventingBasicConsumer(_consumerChannel);
            consumer.Received += Consumer_Received;

            _consumerChannel.BasicConsume
            (
                queue: queue,
                autoAck: false,
                consumer: consumer
            );

            _logger.LogTrace("Started RabbitMQ basic consume.");
        }

        private async Task Consumer_Received(object sender, BasicDeliverEventArgs eventArgs)
        {
            var eventName = eventArgs.RoutingKey;
            var message = Encoding.UTF8.GetString(eventArgs.Body.Span);

            bool isAcknowledged = false;

            try
            {
                await ProcessEvent(eventName, message);

                _consumerChannel.BasicAck(eventArgs.DeliveryTag, multiple: false);
                isAcknowledged = true;
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "Error processing the following message: {Message}.", message);
            }
            finally
            {
                if (!isAcknowledged)
                {
                    await TryEnqueueMessageAgainAsync(eventArgs);
                }
            }
        }

        private async Task TryEnqueueMessageAgainAsync(BasicDeliverEventArgs eventArgs)
        {
            try
            {
                _logger.LogWarning("Adding message to queue again with {Time} seconds delay...", $"{_subscribeRetryTime.TotalSeconds:n1}");

                await Task.Delay(_subscribeRetryTime);
                _consumerChannel.BasicNack(eventArgs.DeliveryTag, false, true);

                _logger.LogTrace("Message added to queue again.");
            }
            catch (Exception ex)
            {
                _logger.LogError("Could not enqueue message again: {Error}.", ex.Message);
            }
        }

        private async Task ProcessEvent(string eventName, string message)
        {
            _logger.LogTrace("Processing RabbitMQ event: {EventName}...", eventName);

            if (!_subscriptionsManager.HasSubscriptionsForEvent(eventName))
            {
                _logger.LogTrace("There are no subscriptions for this event.");
                return;
            }

            var subscriptions = _subscriptionsManager.GetHandlersForEvent(eventName);
            foreach (var subscription in subscriptions)
            {
                var handler = _serviceProvider.GetService(subscription.HandlerType);
                if (handler == null)
                {
                    _logger.LogWarning("There are no handlers for the following event: {EventName}", eventName);
                    continue;
                }

                var eventType = _subscriptionsManager.GetEventTypeByName(eventName);

                var @event = JsonSerializer.Deserialize(message, eventType);
                var eventHandlerType = typeof(IEventHandler<>).MakeGenericType(eventType);
                await Task.Yield();
                await (Task)eventHandlerType.GetMethod(nameof(IEventHandler<Event>.HandleAsync)).Invoke(handler, new object[] { @event });
            }

            _logger.LogTrace("Processed event {EventName}.", eventName);
        }

        private void SubscriptionManager_OnEventRemoved(object sender, string eventName)
        {
            if (!_persistentConnection.IsConnected)
            {
                _persistentConnection.TryConnect();
            }

            if (!_queueMappings.TryGetValue(eventName, out var queueName))
            {
                return;
            }

            using (var channel = _persistentConnection.CreateModel())
            {
                channel.QueueUnbind(queue: queueName, exchange: _exchangeName, routingKey: eventName);

                if (_subscriptionsManager.IsEmpty)
                {
                    _consumerChannel.Close();
                }
            }
        }

        private void AddQueueBindForEventSubscription(string eventName)
        {
            var containsKey = _subscriptionsManager.HasSubscriptionsForEvent(eventName);

            if (containsKey || !_queueMappings.TryGetValue(eventName, out var queueName))
            {
                return;
            }

            if (!_persistentConnection.IsConnected)
            {
                _persistentConnection.TryConnect();
            }

            using (var channel = _persistentConnection.CreateModel())
            {
                channel.QueueBind(queue: queueName, exchange: _exchangeName, routingKey: eventName);
            }
        }

        private void PersistentConnection_OnReconnectedAfterConnectionFailure(object sender, EventArgs e)
        {
            DoCreateConsumerChannel();
            RecreateSubscriptions();
        }

        private void DoCreateConsumerChannel()
        {
            _consumerChannel.Dispose();
            _consumerChannel = CreateConsumerChannel();
            StartBasicConsume();
        }

        private void RecreateSubscriptions()
        {
            var subscriptions = _subscriptionsManager.GetAllSubscriptions();
            _subscriptionsManager.Clear();

            Type eventBusType = this.GetType();
            MethodInfo genericSubscribe;

            foreach (var entry in subscriptions)
            {
                foreach (var subscription in entry.Value)
                {
                    genericSubscribe = eventBusType.GetMethod("Subscribe").MakeGenericMethod(subscription.EventType, subscription.HandlerType);
                    genericSubscribe.Invoke(this, null);
                }
            }
        }
    }
}
