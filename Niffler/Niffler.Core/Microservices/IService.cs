#region Includesusing Niffler.AMQPusing Niffler.Messaging.RabbitMQ;
#endregion

namespace Niffler.Microservices
{
    internal interface IService {
        void Init();
        void ShutDown();
    }
}