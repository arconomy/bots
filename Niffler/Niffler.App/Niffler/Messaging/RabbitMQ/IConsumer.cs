#region Includes

using System;
using System.Collections.Generic;
using System.Text;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;
using Niffler.Messaging.AMQP;
using Niffler.Microservices;
using Niffler.Messaging.Protobuf;

#endregion

namespace Niffler.Messaging.RabbitMQ {
    public abstract class IConsumer : IConsumerService {

       
        protected readonly IModel Channel; 
        protected readonly ushort PrefetchCount;
        protected readonly bool AutoAck;
        protected readonly int Timeout;
        protected readonly IDictionary<string, object> QueueArgs;
        protected string ExchangeName;
        protected string QueueName;
        protected readonly string ExchangeType;
        protected List<RoutingKey> RoutingKeys;
        protected readonly string BotRoutingKey;
        protected readonly IDictionary<string, string> BotConfig;

        protected volatile bool StopConsuming;

        public IConsumer(IDictionary<string, string> botConfig, string queueName, bool autoscale = false,
                       int timeout = 10, ushort prefetchCount = 1, bool autoAck = true,
                                   IDictionary<string, object> queueArgs = null) : base()
        {
            this.BotConfig = botConfig;
            Channel = Adapter.GetConnection().CreateModel();
            BotConfig.TryGetValue("Market", out ExchangeName);
            this.ExchangeType = Exchange.GetExchangeType(RabbitMQ.ExchangeType.TOPIC);
            this.QueueName = queueName;
            this.PrefetchCount = prefetchCount;
            this.AutoAck = autoAck;
            this.Timeout = timeout;
            this.QueueArgs = queueArgs;
        }

        public IConsumer(IDictionary<string, string> botConfig, bool autoScale = false,
                        int timeout = 10, ushort prefetchCount = 1, bool autoAck = true,
                                    IDictionary<string, object> queueArgs = null) : base()
        {
            this.BotConfig = botConfig;
            Channel = Adapter.GetConnection().CreateModel();
            this.BotConfig.TryGetValue("Market", out ExchangeName);
            this.ExchangeType = Exchange.GetExchangeType(RabbitMQ.ExchangeType.TOPIC);
            this.PrefetchCount = prefetchCount;
            this.AutoAck = autoAck;
            this.Timeout = timeout;
            this.QueueArgs = queueArgs;
            if(autoScale)
            {
                InitAutoScale(QueueName);
            }
        }
       
        public override void Start() {
            StopConsuming = false;
            
            try
            {
                using (Channel)
                {
                    Channel.ExchangeDeclare(exchange: ExchangeName, type: ExchangeType);
                    if(QueueName == null || QueueName == "")
                    {
                        QueueName = Channel.QueueDeclare().QueueName;
                    }
                    else
                    {
                        QueueName = Channel.QueueDeclare(QueueName);
                    }

                    RoutingKeys = GetListeningRoutingKeys();

                    foreach (RoutingKey routingKey in RoutingKeys)
                    {
                        Channel.QueueBind(queue: QueueName, exchange: ExchangeName, routingKey: routingKey.GetRoutingKey());
                    }

                    Channel.BasicQos(0, PrefetchCount, false);

                    var consumer = new EventingBasicConsumer(Channel);
                    while (!StopConsuming)
                    {
                        try
                        {
                            consumer.Received += (model, ea) =>
                            {
                                    MessageReceived(new MessageReceivedEventArgs()
                                    {   
                                        Message = Niffle.Parser.ParseFrom(ea.Body),
                                        EventArgs = ea
                                    }
                                );
                                Console.WriteLine(" [x] Received '{0}':'{1}'", ea.RoutingKey, Encoding.UTF8.GetString(ea.Body));
                            };
                            Channel.BasicConsume(queue: QueueName, autoAck: AutoAck, consumer: consumer);
                        }
                        catch (Exception exception)
                        {
                            MessageReceived(new MessageReceivedEventArgs
                            {
                                Exception = new AMQPConsumerProcessingException(exception)
                            });
                        }
                    }
                }
            }
            catch (Exception exception) {
                MessageReceived(new MessageReceivedEventArgs {
                    Exception = new AMQPConsumerInitialisationException(exception)
                });
            }
        }

        public override void Stop()
        {
            StopConsuming = true;
        }

        protected abstract List<RoutingKey> GetListeningRoutingKeys();
    }
}