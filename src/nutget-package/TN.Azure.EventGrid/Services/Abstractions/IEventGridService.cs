namespace TN.Azure.EventGrid.Services.Abstractions
{
    public interface IEventGridService
    {
        Task SendAsync<T>(T data, string publisherClientName, string subject, string eventType, string dataVersion);
    }
}
