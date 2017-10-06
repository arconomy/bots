#region Includes

using System;

#endregion

namespace Niffler.Messaging.AMQP {
    [Serializable]
    public class AMQPConsumerInitialisationException : Exception {
        public AMQPConsumerInitialisationException(Exception innerException) :
            base("An Exception occured while initialising the AMQPConsumer.", innerException) {}
    }
}