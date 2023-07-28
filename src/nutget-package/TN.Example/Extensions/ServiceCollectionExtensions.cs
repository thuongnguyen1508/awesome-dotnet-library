using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Generic;
using System.Reflection;
using TN.Application.IntegrationEvents.EventHandlers;
using TN.EventBus.RabbitMQ.Extensions;
using TN.Example.Application.IntegrationEvents.EventHandlers;

namespace TN.Example.Extensions
{
    public static class ServiceCollectionExtensions
    {
        /// <summary>
        /// Register RabbitMQ service to Autofac container
        /// </summary>
        /// <param name="services"></param>
        /// <param name="configuration"></param>
        /// <param name="definedEventTypes">It's param for specify which events will be consumed</param>
        /// <returns></returns>
        public static IServiceCollection AddEventBus(this IServiceCollection services, IConfiguration configuration, Dictionary<Type, string> definedMessageTypes = null)
        {
            services.AddRabbitMQEventBus(configuration, Assembly.GetExecutingAssembly(), definedMessageTypes);
            return services;
        }
    }
}
