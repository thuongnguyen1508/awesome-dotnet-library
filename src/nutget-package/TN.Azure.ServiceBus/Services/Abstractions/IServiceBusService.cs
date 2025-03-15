namespace TN.Azure.ServiceBus.Services.Abstractions
{
    public interface IServiceBusService
    {
        Task SendAsync<T>(T item, string topic, Dictionary<string, object>? messageProperties = null);
    }
}
