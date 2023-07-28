using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Hosting;
using System;
using System.IO;
using System.Threading.Tasks;
using TN.Hosts.Extensions;

namespace TN.Example
{
    public class Program
    {
        public static readonly string EnvironmentName = Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT");

        public static async Task<int> Main(string[] args)
        {
            var configuration = GetConfiguration(null);
            var host = BuildWebHost<Startup>(configuration, args);

            await host.RunWithTasksAsync();
            return 0;
        }

        public static IHost BuildWebHost<TStartup>(IConfiguration configuration, string[] args) where TStartup : class
        {
            return Host.CreateDefaultBuilder(args)
                  .ConfigureAppConfiguration((builderContext, builder) =>
                  {
                      BuildConfiguration(builder);
                  })
                 .ConfigureWebHostDefaults(webBuilder =>
                 {
                     webBuilder
                     .CaptureStartupErrors(false)
                     .UseKestrel()
                     .UseStartup<TStartup>()
                     .UseConfiguration(configuration)
                     .UseContentRoot(Directory.GetCurrentDirectory())
                     .ConfigureKestrel(serverOptions =>
                     {
                     });
                 })
                 .Build();
        }

        public static IConfiguration GetConfiguration(string[] extraConfigFolders = null)
        {
            var currentDirectory = Directory.GetCurrentDirectory();
            var builder = new ConfigurationBuilder()
                .SetBasePath(currentDirectory);

            BuildConfiguration(builder);

            return builder.Build();
        }
        public static void BuildConfiguration(IConfigurationBuilder builder)
        {
            builder.AddJsonFile("appsettings.json", optional: false, reloadOnChange: true);
            builder.AddJsonFile($"appsettings.{EnvironmentName}.json", optional: true, reloadOnChange: true);
            builder.AddJsonFile($"appsettings.local.json", optional: true, reloadOnChange: true);
            builder.AddEnvironmentVariables();
        }
    }
}
