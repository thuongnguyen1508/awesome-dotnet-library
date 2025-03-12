namespace TN.Azure.EventHub.Services.Abstractions
{
    public interface IEvenHubService
    {
        Task SendAsync<T>(T item, string eventHubName, Dictionary<string, object>? eventProperties = null);
    }
}
