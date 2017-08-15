#region Includes

using System;
using System.Collections.Generic;
using System.Text;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;
using Niffler.Messaging.AMQP;

#endregion

namespace Niffler.Messaging.RabbitMQ {
    public class Consumer {

        protected readonly IModel Channel;
        protected readonly String ExchangeName;
        protected readonly String ExchangeType;
        protected readonly string QueueName;
        protected readonly string[] RoutingKeys;
        protected readonly ushort PrefetchCount;
        protected readonly bool AutoAck;
        protected readonly int Timeout;
        protected readonly IDictionary<string, object> QueueArgs;
        protected volatile bool StopConsuming;



        public Consumer(IConnection connection, string exchangeName, string exchangeType, string queueName, string[] routingKeys,
                                    int timeout, ushort prefetchCount = 1, bool autoAck = true,
                                    IDictionary<string, object> queueArgs = null)
        {
            this.Channel = connection.CreateModel();
            this.ExchangeName = exchangeName;
            this.ExchangeType = exchangeType;
            this.QueueName = queueName;
            this.RoutingKeys = routingKeys;
            this.PrefetchCount = prefetchCount;
            this.AutoAck = autoAck;
            this.Timeout = timeout;
            this.QueueArgs = queueArgs;
        }

        public Consumer Clone(Adapter adapter)
        {
           return new Consumer((IConnection) adapter.GetConnection(), ExchangeName, ExchangeType, QueueName, RoutingKeys, Timeout);
        }


        public void Start(Adapter adapter) {
            StopConsuming = false;

            try
            {
                using (Channel)
                {
                    Channel.ExchangeDeclare(exchange: ExchangeName, type: ExchangeType);
                    Channel.QueueDeclare(QueueName, true, false, false, QueueArgs);

                    foreach (string routingKey in RoutingKeys)
                    {
                        Channel.QueueBind(queue: QueueName, exchange: ExchangeName, routingKey: routingKey);
                    }

                    Channel.BasicQos(0, PrefetchCount, false);

                    var consumer = new EventingBasicConsumer(Channel);
                    while (!StopConsuming)
                    {
                        try
                        {
                            consumer.Received += (model, ea) =>
                            {
                                OnMessageReceived(new MessageReceivedEventArgs()
                                    {
                                        Message = Encoding.UTF8.GetString(ea.Body),
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
                                Exception = new AMQPConsumerProcessingException(exception)
                            });
                        }
                    }
                }
            }
            catch (Exception exception) {
                OnMessageReceived(new MessageReceivedEventArgs {
                    Exception = new AMQPConsumerInitialisationException(exception)
                });
            }
        }

        public void Stop()
        {
            StopConsuming = true;
        }

        protected void OnMessageReceived(MessageReceivedEventArgs e)
        {
            MessageReceived?.Invoke(this, e);
        }

        public event EventHandler<MessageReceivedEventArgs> MessageReceived;
    }
}