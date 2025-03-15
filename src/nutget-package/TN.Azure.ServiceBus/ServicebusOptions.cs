namespace TN.Azure.ServiceBus
{
    public class ServicebusOptions
    {
        public string EndPoint { get; set; }
        public bool UseAzureIdentity { get; set; } = true;
        public string ConnectionString { get; set; }
    }
}
