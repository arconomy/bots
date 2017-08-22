#region Includes

using System;
using System.Collections.Generic;
using Niffler.Messaging.RabbitMQ;
using RabbitMQ.Client;
using Niffler.Common;

#endregion

namespace Niffler.Microservices {
    public class AutoScaleManager : IConsumer {

        private State State;

        public AutoScaleManager(IDictionary<string, string> config, string queueName)
            : base(config, "topic", false, new string[] { queueName + ".AutoScale.*" })
        {
           
        }

        public override object Clone()
        {
            return new AutoScaleManager(BotConfig, QueueName);
        }

        public override void Init()
        {
            
        }

        public override void MessageReceived(MessageReceivedEventArgs e)
        {
            if (e.Message.Contains("scale-out"))
            {
                var newConsumer = (IConsumer)Clone();
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
    }
}