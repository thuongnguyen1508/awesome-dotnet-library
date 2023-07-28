using System;
using System.Collections.Generic;

namespace TN.EventBus.Subscriptions
{
    public class DefinedSubscriptionMessageType
    {
        public DefinedSubscriptionMessageType()
        {
        }

        public DefinedSubscriptionMessageType(Dictionary<Type, string> messageTypes)
        {
            //MessageType = key;
            //MessageName = value;
            MessageTypes = messageTypes;
        }

        public Dictionary<Type, string> MessageTypes { get; set; }

        //public Type MessageType { get; set; }
        //public string MessageName { get; set; }
    }
}