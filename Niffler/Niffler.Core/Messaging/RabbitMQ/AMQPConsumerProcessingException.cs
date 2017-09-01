#region Includes
using System;
using System.Collections.Generic;
using Niffler.Messaging.RabbitMQ;
#endregion

namespace Niffler.Messaging.AMQP {
    [Serializable]
    public class AMQPConsumerProcessingException : Exception {
        public AMQPConsumerProcessingException(Exception innerException, string exchangeName, List<RoutingKey> routingKeys) :
            base("An Exception occured while consuming the Queue on Exchange: " + exchangeName + " with Routing Key(s): " + routingKeys.ToString(), innerException) {}
    }
}