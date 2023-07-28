using System.Collections.Generic;

namespace TN.EventBus.RabbitMQ
{
    public class RabbitMQSettings
    {
        public string ConnectionUrl { get; set; }
        public string ExchangeName { get; set; }
        public int TimeoutBeforeReconnecting { get; set; }
        public IDictionary<string, string> QueueMappings { get; set; }
    }
}
