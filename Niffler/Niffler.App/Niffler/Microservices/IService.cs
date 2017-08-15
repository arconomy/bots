#region Includes

using Daishi.AMQP;
using Niffler.Messaging.RabbitMQ;
using System;

#endregion

namespace Niffler.Microservices {
    internal interface IService {
        void Init();
        void OnMessageReceived(Object sender, MessageReceivedEventArgs e);
        void Shutdown();
    }
}