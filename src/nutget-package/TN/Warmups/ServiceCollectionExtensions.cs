using Microsoft.Extensions.DependencyInjection;
using TN.Warmups;

namespace TN.Warmups
{
    public static class ServiceCollectionExtensions
    {
        public static IServiceCollection AddStartupTask<T>(this IServiceCollection services)
              where T : class, IStartupTask
              => services.AddTransient<IStartupTask, T>();
    }
}