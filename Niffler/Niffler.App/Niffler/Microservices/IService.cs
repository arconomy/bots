#region Includesusing Niffler.AMQPusing Niffler.Messaging.RabbitMQ;
using Niffler.Messaging.RabbitMQ;
#endregion

namespace Niffler.Microservices {
    internal interface IService {
        bool Init();
        void Shutdown();
        //Using inheritance not Event handlers
        void OnMessageReceived(object sender, MessageReceivedEventArgs e);
    }
}