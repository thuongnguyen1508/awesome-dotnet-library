using Microsoft.Extensions.DependencyInjection;
using TN.Excel.Services;
using TN.Excel.Services.Abstractions;

namespace TN.Excel
{
    public static class ServiceCollectionExtension
    {
        public static IServiceCollection AddExcelService(this IServiceCollection services)
        {
            services.AddTransient<IExcelService, ExcelService>();
            return services;
        }
    }
}
