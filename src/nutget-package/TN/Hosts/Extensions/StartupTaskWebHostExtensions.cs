using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using System.Threading;
using System.Threading.Tasks;
using TN.Warmups;

namespace TN.Hosts.Extensions
{
    public static class StartupTaskWebHostExtensions
    {
        public static async Task RunWithTasksAsync(this IHost webHost, CancellationToken cancellationToken = default)
        {
            // Load all tasks from DI
            var startupTasks = webHost.Services.GetServices<IStartupTask>();

            // Execute all the tasks
            foreach (var startupTask in startupTasks)
            {
                await startupTask.ExecuteAsync(cancellationToken);
            }

            // Start the tasks as normal
            await webHost.RunAsync(cancellationToken);
        }
    }
}
