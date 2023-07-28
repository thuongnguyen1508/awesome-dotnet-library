using RabbitMQ.Client;
using System;

namespace TN.EventBus.RabbitMQ.Connection
{
    public interface IPersistentConnection
    {
        event EventHandler OnReconnectedAfterConnectionFailure;
        bool IsConnected { get; }

        bool TryConnect();
        IModel CreateModel();
    }
}
