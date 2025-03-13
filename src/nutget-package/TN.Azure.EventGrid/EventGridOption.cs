namespace TN.Azure.EventGrid
{
    public class EventGridOption
    {
        public string Endpoint { get; set; }
        public string Key { get; set; }
        public List<string> EventGridPublisherNameDefinition { get; set; } = new();
    }
}
