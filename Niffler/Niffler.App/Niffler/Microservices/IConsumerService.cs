#region Includes

using Niffler.Messaging.RabbitMQ;
using RabbitMQ.Client;
using System;
using System.Collections.Generic;

#endregion

namespace Niffler.Microservices {
        public abstract class IConsumerService : IService, ICloneable {
        protected readonly List<IConsumer> Consumers = new List<IConsumer>();
        private IConsumer AutoScaleConsumer;

        protected Adapter Adapter;
        protected readonly IConnection Connection;

        public IConsumerService()
        {
            Adapter = Adapter.Instance;
            Adapter.Init();
            Adapter.Connect();
            Connection = Adapter.GetConnection();
        }

        protected void InitAutoScale(string queueName)
        {
            //Create an autoscale consumer to listen to messages to scale consumers - need to tell the consumers listening to a particular Queue to scale-out
            Dictionary<string, string> autoScaleConfig = new Dictionary<string, string>
            {
                { "Market", "AutoScaleX" }
            };
            AutoScaleConsumer = new AutoScaleManager(autoScaleConfig, queueName);
            Adapter.ConsumeAsync(AutoScaleConsumer);
        }
     
        public void Shutdown()
        {
            if (Adapter == null) return;

            if (Consumers != null && Consumers.Count > 0)
            {
                foreach (var consumer in Consumers)
                {
                    Adapter.StopConsumingAsync(consumer);
                }
                Consumers.Clear();
            }
            Adapter.Disconnect();
        }

        public abstract object Clone();
        public abstract bool Init();
        public abstract void Start();
        public abstract void Stop();
        public abstract void MessageReceived(MessageReceivedEventArgs e);
    }
}