using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
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
        //public static IServiceCollection AddRabbitMQService(this IServiceCollection services, IConfiguration configuration, Dictionary<Type, string> definedEventTypes)
        //{
        //    services.Configure<RabbitMQSettings>(op =>
        //    {
        //        configuration.GetSection("RabbitMQ").Bind(op);
        //    });

        //    //DI IRabbitMQService
        //    services.AddSingleton<IRabbitMQService, RabbitMQService>(factory =>
        //    {
        //        var rabbitMQSeting = factory.GetRequiredService<IOptions<RabbitMQSetting>>().Value;
        //        var connectionFactory = new ConnectionFactory
        //        {
        //            Uri = new Uri(rabbitMQSeting.ConnectionUrl),
        //            DispatchConsumersAsync = true,
        //        };
        //        var logger = factory.GetService<ILogger<RabbitMQService>>();

        //        return new RabbitMQService(connectionFactory, logger, rabbitMQSeting, definedEventTypes, factory);
        //    });
        //    return services;
        //}

        public static IServiceCollection AddEventBus(this IServiceCollection services, IConfiguration configuration)
        {
            services.AddRabbitMQEventBus(configuration);

            services.AddTransient<MessageSentEventHandler>();
            services.AddTransient<MessageDeliveredEventHandler>();
            return services;
        }
    }
}
