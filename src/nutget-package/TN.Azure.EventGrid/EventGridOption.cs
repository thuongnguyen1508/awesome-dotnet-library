namespace TN.Azure.EventGrid
{
    public class EventGridOption
    {
        public string Endpoint { get; set; }
        public string Key { get; set; }
        public bool UseAzureIdentity { get; set; } = true;
    }
}
