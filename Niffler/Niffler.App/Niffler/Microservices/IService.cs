#region Includesusing Niffler.AMQPusing Niffler.Messaging.RabbitMQ;
using Niffler.Messaging.RabbitMQ;
using System;
using System.Collections.Generic;

#endregion

namespace Niffler.Microservices {
    internal interface IService {
        void Init();
        void Start();
        void Stop();
        void Shutdown();
        //Using inheritance not Event handlers
        void MessageReceived(MessageReceivedEventArgs e);
    }
}