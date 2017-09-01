#region Includes

using System;
using System.Collections.Generic;
using Niffler.Messaging.RabbitMQ;
using RabbitMQ.Client;
using Niffler.Common;
using Niffler.Data;
using Niffler.Strategy;
using Niffler.Messaging.Protobuf;
using Niffler.Rules;

#endregion

namespace Niffler.RabbitMQ
{
    public class ConsumerAutoScaleManager
    {
        private Adapter Adapter;
        private Consumer ScalableConsumer;
        private List<Consumer> ScalableConsumers = new List<Consumer>();
        private Consumer AutoScaleConsumer;

        public ConsumerAutoScaleManager(Consumer scalableConsumer)
        {
            ScalableConsumer = scalableConsumer;
            Adapter = scalableConsumer.GetAdapter();
            ScalableConsumers.Add(scalableConsumer);
        }

        public void Init()
        {
            AutoScaleConsumer = new Consumer(Adapter, Exchange.AUTOSCALEX, RoutingKey.Create(ScalableConsumer.GetQueueName()).ToList());
            AutoScaleConsumer.Init();
            AutoScaleConsumer.MessageReceived += OnMessageReceived;
            Adapter.ConsumeAsync(AutoScaleConsumer);
        }

        public void OnMessageReceived(Object o, MessageReceivedEventArgs e)
        {
            if (e.Message.Type == Niffle.Types.Type.Service)
            {
                if (e.Message.Service != null)
                {
                    switch (e.Message.Service.Command)
                    {
                        case Service.Types.Command.Scaleup:
                            {
                                ScaleUp();
                                break;
                            }
                        case Service.Types.Command.Scaledown:
                            {
                                ScaleDown();
                                break;
                            }
                        case Service.Types.Command.Shutdown:
                            {
                                ShutDown();
                                break;
                            }
                    }
                }
            }
        }

        public void ScaleUp()
        {
            Consumer newConsumer = (Consumer) ScalableConsumer.Clone();
            newConsumer.Init();
            Adapter.ConsumeAsync(newConsumer);
            ScalableConsumers.Add(newConsumer);
        }

        public void ScaleDown()
        {
            if (ScalableConsumers.Count <= 1) return;
            var lastConsumer = ScalableConsumers[ScalableConsumers.Count - 1];
            Adapter.StopConsumingAsync(lastConsumer);
            ScalableConsumers.RemoveAt(ScalableConsumers.Count - 1);
        }
       
        public void ShutDown()
        {
            if (Adapter == null) return;

            if (ScalableConsumers != null && ScalableConsumers.Count > 0)
            {
                foreach (var consumer in ScalableConsumers)
                {
                    Adapter.StopConsumingAsync(consumer);
                }
                ScalableConsumers.Clear();
            }
        }
    }
}