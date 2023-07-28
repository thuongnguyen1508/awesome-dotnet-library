using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.DependencyInjection;
using TN.EventBus;

namespace TN.Example.Extensions
{
    public static class ConfigurationExtension
    {
        /// <summary>
        /// Invoke SubscribeWarmup() method of IRabbitMQService
        /// </summary>
        /// <param name="app"></param>
        //public static void UseRabbitMQ(this IApplicationBuilder app)
        //{
        //    var rabbitMQService = app.ApplicationServices.GetRequiredService<IEventBus>();
        //    rabbitMQService.SubscribeWarmup();
        //}
    }
}
