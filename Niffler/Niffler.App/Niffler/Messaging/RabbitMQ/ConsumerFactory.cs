#region Includes

using RabbitMQ.Client;
using System.Collections.Generic;

#endregion

namespace Niffler.Messaging.RabbitMQ {
    public static class ConsumerFactory {

        public static Consumer CreateConsumer(IConnection connection, string exchangeName, string exchangeType, string queueName, string[] routingKeys,
                                                        int timeout, ushort prefetchCount = 1, bool autoAck = true,
                                                        IDictionary<string, object> queueArgs = null)
        { 
            return new Consumer(connection, exchangeName, exchangeType,queueName, routingKeys, timeout, prefetchCount, autoAck, queueArgs);
        }

        public static ConsumerConfig CreateConsumerConfig(string exchangeName, string exchangeType, string queueName, string[] routingKeys)
        {
            return new ConsumerConfig(exchangeName, exchangeType, queueName, routingKeys);
        }
    }
}