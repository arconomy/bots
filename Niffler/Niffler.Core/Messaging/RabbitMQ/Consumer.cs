#region Includes

using System;
using System.Collections.Generic;
using System.Text;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;
using Niffler.Messaging.AMQP;
using Niffler.Messaging.Protobuf;
using System.Threading;
using RabbitMQ.Client.MessagePatterns;

#endregion

namespace Niffler.Messaging.RabbitMQ
{
    public class Consumer : DefaultBasicConsumer, ICloneable
    {
        protected readonly Adapter Adapter;
        protected string ExchangeName;
        protected ExchangeType ExchangeType = ExchangeType.TOPIC;
        protected string QueueName;
        protected List<RoutingKey> RoutingKeys;

        protected readonly int Timeout;
        protected readonly ushort PrefetchCount;
        protected readonly bool AutoAck;
        protected readonly IDictionary<string, object> QueueArgs;
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
 
        public void Start() {
            try
            {
                //Channel runs in it's own thread
                Model = Adapter.GetConnection().CreateModel();
                Model.ExchangeDeclare(exchange: ExchangeName, type: Exchange.GetExchangeType(ExchangeType));
                if (String.IsNullOrEmpty(QueueName))
                {
                    QueueName = Model.QueueDeclare(durable: false, exclusive: true, autoDelete: true).QueueName;
                }
                else
                {
                    //For Autoscaling need to use the existing queue
                    QueueName = Model.QueueDeclare(QueueName, false, true, true);
                }

                foreach (RoutingKey routingKey in RoutingKeys)
                {
                    Model.QueueBind(queue: QueueName, exchange: ExchangeName, routingKey: routingKey.GetRoutingKey());
                }

                Model.BasicQos(0, PrefetchCount, false);
                Model.BasicConsume(queue: QueueName, autoAck: AutoAck, consumer: this);
            }
            catch (Exception exception) {
                OnMessageReceived(new MessageReceivedEventArgs {
                    Exception = new AMQPConsumerInitialisationException(exception, ExchangeName, RoutingKeys)
                });
            }
        }

        public override void HandleBasicDeliver(string consumerTag, ulong deliveryTag, bool redelivered, string exchange, string routingKey, IBasicProperties properties, byte[] body)
        {
            //All msgs are At-most-once delivery
            if (!redelivered)
            {
                OnMessageReceived(new MessageReceivedEventArgs()
                {
                    Message = Niffle.Parser.ParseFrom(body),
                    EventArgs = new BasicDeliverEventArgs()
                        {
                            ConsumerTag = consumerTag,
                            DeliveryTag = deliveryTag,
                            Redelivered = redelivered,
                            Exchange = exchange,
                            RoutingKey = routingKey,
                            BasicProperties = properties
                    }

                });
                Console.WriteLine(" [x] Received '{0}':'{1}'", routingKey);
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
            if(IsRunning)
            {
                Model.BasicCancel(ConsumerTag);
            }
            Model.Close();
            Model.Dispose();
        }


        public object Clone()
        {
            return new Consumer(Adapter, ExchangeName, RoutingKeys,QueueName,Timeout,PrefetchCount,AutoAck,QueueArgs);
        }
    }
}