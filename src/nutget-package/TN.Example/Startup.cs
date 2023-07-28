using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.OpenApi.Models;
using System;
using System.Collections.Generic;
using TN.Application.IntegrationEvents.EventHandlers;
using TN.EventBus;
using TN.Example.Application.IntegrationEvents.EventHandlers;
using TN.Example.Application.IntegrationEvents.Events;
using TN.Example.Extensions;

namespace TN.Example
{
    public class Startup
    {
        public Startup(IConfiguration configuration)
        {
            Configuration = configuration;
        }

        public IConfiguration Configuration { get; }

        // This method gets called by the runtime. Use this method to add services to the container.
        public void ConfigureServices(IServiceCollection services)
        {

            services.AddControllers();
            services.AddSwaggerGen(c =>
            {
                c.SwaggerDoc("v1", new OpenApiInfo { Title = "TN.Example", Version = "v1" });
            });
            services.AddEventBus(Configuration, new Dictionary<Type, string>
            {
                //Specify which events will be consumed. We can manage consume process base on it, if it empty => have no consume process.
                //Base on typeof(MessageSentEvent) , it will automatic invoke MessageSentEventHandler to process when each event recieved.
                [typeof(MessageSentEvent)] = "chat:queue:message",
                [typeof(MessageDeliveredEvent)] = "chat:queue:message:delivered",
            });
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
        {
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
                app.UseSwagger();
                app.UseSwaggerUI(c => c.SwaggerEndpoint("/swagger/v1/swagger.json", "TN.Example v1"));
            }

            app.UseHttpsRedirection();

            app.UseRouting();

            app.UseAuthorization();

            app.UseEndpoints(endpoints =>
            {
                endpoints.MapControllers();
            });
        }
    }
}
