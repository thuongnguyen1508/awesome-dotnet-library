namespace TN.Azure.EventHub
{
    public class EventHubOptions
    {
        public string EndPoint { get; set; }
        public bool UseAzureIdentity { get; set; } = true;
        public string ConnectionString { get; set; }
    }
}
