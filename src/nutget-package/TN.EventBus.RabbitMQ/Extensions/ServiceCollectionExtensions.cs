using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using RabbitMQ.Client;
using System;
using System.Collections.Generic;
using TN.EventBus.RabbitMQ.Bus;
using TN.EventBus.RabbitMQ.Connection;
using TN.EventBus.Subscriptions;
using TN.Warmups;

namespace TN.EventBus.RabbitMQ.Extensions
{
    public static class ServiceCollectionExtensions
    {
        public static IServiceCollection AddRabbitMQEventBus(this IServiceCollection services, IConfiguration configuration, Dictionary<Type, string> definedMessageTypes = null)
        {
            definedMessageTypes ??= new Dictionary<Type, string>();
            services.AddSingleton(sp => new DefinedSubscriptionMessageType(definedMessageTypes));

            services.Configure<RabbitMQSettings>(op =>
            {
                configuration.GetSection("RabbitMQ").Bind(op);
            });
            services.AddSingleton<EventBusSubscriptionManager, EventBusSubscriptionManager>();
            services.AddSingleton<IPersistentConnection, RabbitMQPersistentConnection>(factory =>
            {
                var rabbitMQSeting = factory.GetRequiredService<IOptions<RabbitMQSettings>>().Value;
                var connectionFactory = new ConnectionFactory
                {
                    Uri = new Uri(rabbitMQSeting.ConnectionUrl),
                    DispatchConsumersAsync = true,
                };

                var logger = factory.GetService<ILogger<RabbitMQPersistentConnection>>();
                return new RabbitMQPersistentConnection(connectionFactory, logger, rabbitMQSeting.TimeoutBeforeReconnecting);
            });

            services.AddSingleton<IEventBus, RabbitMQEventBus>(factory =>
            {
                var persistentConnection = factory.GetService<IPersistentConnection>();
                var subscriptionManager = factory.GetService<EventBusSubscriptionManager>();
                var logger = factory.GetService<ILogger<RabbitMQEventBus>>();
                var rabbitMQSeting = factory.GetRequiredService<IOptions<RabbitMQSettings>>().Value;

                return new RabbitMQEventBus(persistentConnection, subscriptionManager, factory, logger, rabbitMQSeting);
            });

            services.AddStartupTask<EventBusSubscribeWarmup>();

            return services;
        }
    }
}
