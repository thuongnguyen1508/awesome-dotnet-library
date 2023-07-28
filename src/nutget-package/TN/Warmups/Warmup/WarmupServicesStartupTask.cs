using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace TN.Warmups.Warmup
{
    public class WarmupServicesStartupTask : IStartupTask
    {
        private readonly IServiceCollection _services;
        private readonly IServiceProvider _provider;

        public WarmupServicesStartupTask(IServiceProvider provider, IServiceCollection services)
        {
            _services = services;
            _provider = provider;
        }

        public Task ExecuteAsync(CancellationToken cancellationToken)
        {
            using (var scope = _provider.CreateScope())
            {
                foreach (var singleton in GetServices(_services))
                {
                    scope.ServiceProvider.GetServices(singleton);
                }
            }

            return Task.CompletedTask;
        }

        private static IEnumerable<Type> GetServices(IServiceCollection services)
        {
            return services
                .Where(descriptor => descriptor.Lifetime == ServiceLifetime.Singleton)
                .Where(descriptor => descriptor.ImplementationType != typeof(WarmupServicesStartupTask))
                .Where(descriptor => descriptor.ServiceType.ContainsGenericParameters == false)
                .Select(descriptor => descriptor.ServiceType)
                .Distinct();
        }
    }
}
