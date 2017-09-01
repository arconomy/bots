#region Includes

using Niffler.Messaging.RabbitMQ;
using System;
using System.Collections.Generic;

#endregion

namespace Niffler.Messaging.AMQP {
    [Serializable]
    public class AMQPConsumerInitialisationException : Exception {
        public AMQPConsumerInitialisationException(Exception innerException,string exchangeName, List<RoutingKey> routingKeys) :
            base("An Exception occured while initialising the AMQPConsumer on Exchange: " + exchangeName + " with Routing Key(s): " + routingKeys.ToString(), innerException) {}
    }
}