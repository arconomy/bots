#region Includes

using Niffler.Messaging.RabbitMQ;
using RabbitMQ.Client;
using System;
using System.Collections.Generic;

#endregion

namespace Niffler.Microservices {
        public abstract class IConsumerService : IService {
        private Adapter Adapter;
        private readonly List<Consumer> Consumers = new List<Consumer>();
        private ConsumerConfig ConsumerConfig { get; set; }

        private Consumer Consumer;
        private Consumer AutoScaleConsumer;

        public abstract ConsumerConfig SetConsumerConfig();

        public void OnMessageReceived(object sender, MessageReceivedEventArgs e)
        {
            throw new NotImplementedException();
        }

        public IConsumerService()
        {
            Adapter = Adapter.Instance;
            Adapter.Init();
            Adapter.Connect();
            Init();
        }

        public void Init()
        {
            var defaultMsgTimeOut = Convert.ToInt32(System.Configuration.ConfigurationManager.AppSettings["rabbitmq-default-msg-timeout"]);
            ConsumerConfig = SetConsumerConfig();
            Consumer = new Consumer((IConnection)Adapter.GetConnection(), ConsumerConfig.ExchangeName, ConsumerConfig.ExchangeType, ConsumerConfig.QueueName, ConsumerConfig.RoutingKeys, defaultMsgTimeOut);
            Consumer.MessageReceived += OnMessageReceived;

            AutoScaleConsumer = new Consumer((IConnection)Adapter.GetConnection(), "AutoScaleX", "direct", "AutoScaleQ", new string[] { "autoscale" }, defaultMsgTimeOut);
            AutoScaleConsumer.MessageReceived += OnAutoScaleConsumerMessageReceived;

            Consumers.Add(Consumer);

            Adapter.ConsumeAsync(AutoScaleConsumer);
            Adapter.ConsumeAsync(Consumer);
        }
        
        private void OnAutoScaleConsumerMessageReceived(object sender, MessageReceivedEventArgs e)
        {
            if (e.Message.Contains("scale-out"))
            {
                var newConsumer = Consumer.Clone(Adapter);
                Adapter.ConsumeAsync(newConsumer);
                Consumers.Add(newConsumer);
            }
            else
            {
                if (Consumers.Count <= 1) return;
                var lastConsumer = Consumers[Consumers.Count - 1];

                Adapter.StopConsumingAsync(lastConsumer);
                Consumers.RemoveAt(Consumers.Count - 1);
            }
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
    }
}