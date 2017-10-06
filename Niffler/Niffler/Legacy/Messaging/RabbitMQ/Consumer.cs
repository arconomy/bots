#region Includes

using System;
using System.Collections.Generic;
using System.Text;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;
using Niffler.Messaging.AMQP;
using Niffler.Microservices;
using Niffler.Messaging.Protobuf;
using Niffler.Strategy;

#endregion

namespace Niffler.Messaging.RabbitMQ {
    public class Consumer : ICloneable {

        protected readonly Adapter Adapter;
        protected IModel Channel;
        protected string ExchangeName;
        protected ExchangeType ExchangeType = ExchangeType.TOPIC;
        protected string QueueName;
        protected List<RoutingKey> RoutingKeys;

        protected readonly int Timeout;
        protected readonly ushort PrefetchCount;
        protected readonly bool AutoAck;
        protected readonly IDictionary<string, object> QueueArgs;

        protected volatile bool StopConsuming;
        protected bool IsInitialised = false;
        protected readonly IConnection Connection;

        public event EventHandler<MessageReceivedEventArgs> MessageReceived;

        public Consumer(Adapter adapter,string exchangeName, List<RoutingKey> routingKeys, string queueName = null,
                       int timeout = 10, ushort prefetchCount = 1, bool autoAck = true,
                                   IDictionary<string, object> queueArgs = null) : base()
        {
            this.Adapter = adapter;
            this.ExchangeName = exchangeName;
            this.RoutingKeys = routingKeys;
            if (!String.IsNullOrEmpty(queueName)) QueueName = queueName; //Used for cloning/scaling
            this.Timeout = timeout;
            this.PrefetchCount = prefetchCount;
            this.AutoAck = autoAck;
            this.QueueArgs = queueArgs;
        }

        public void Init()
        {
            //Initialise Channel and queue in order to make queue name available for AutoScaling before starting asynchronously
            Channel = Adapter.GetConnection().CreateModel();
            Channel.ExchangeDeclare(exchange: ExchangeName, type: Exchange.GetExchangeType(ExchangeType));
            if (String.IsNullOrEmpty(QueueName))
            {
                QueueName = Channel.QueueDeclare().QueueName;
            }
            else
            {
                //For Autoscaling need to use the existing queue
                QueueName = Channel.QueueDeclare(QueueName);
            }

            foreach (RoutingKey routingKey in RoutingKeys)
            {
                Channel.QueueBind(queue: QueueName, exchange: ExchangeName, routingKey: routingKey.GetRoutingKey());
            }

            Channel.BasicQos(0, PrefetchCount, false);
            IsInitialised = true;
        }

 
        public void Start() {
            

            StopConsuming = false;
            try
            {
                if (!IsInitialised) throw new AMQPConsumerInitialisationException(new Exception(),ExchangeName,RoutingKeys);

                using (Channel)
                {
                    var consumer = new EventingBasicConsumer(Channel);
                    while (!StopConsuming)
                    {
                        try
                        {
                            consumer.Received += (model, ea) =>
                            {
                                    OnMessageReceived(new MessageReceivedEventArgs()
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
                            OnMessageReceived(new MessageReceivedEventArgs
                            {
                                Exception = new AMQPConsumerProcessingException(exception, ExchangeName, RoutingKeys)
                            });
                        }
                    }
                }
            }
            catch (Exception exception) {
                OnMessageReceived(new MessageReceivedEventArgs {
                    Exception = new AMQPConsumerInitialisationException(exception, ExchangeName, RoutingKeys)
                });
            }
        }

        protected void OnMessageReceived(MessageReceivedEventArgs e)
        {
            MessageReceived?.Invoke(this, e);
        }

        public string GetQueueName()
        {
            return QueueName;
        }

        public Adapter GetAdapter()
        {
            return Adapter;
        }

        public void Stop()
        {
            StopConsuming = true;
        }

        public object Clone()
        {
            return new Consumer(Adapter, ExchangeName, RoutingKeys,QueueName,Timeout,PrefetchCount,AutoAck,QueueArgs);
        }
    }
}