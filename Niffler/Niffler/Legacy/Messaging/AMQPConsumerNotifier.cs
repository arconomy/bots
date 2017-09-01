#region Includes

using Niffler.Messaging.RabbitMQ;
using System.Collections.Concurrent;

#endregion

namespace Niffler.Messaging.AMQP {
    public abstract class AMQPConsumerNotifier {
        protected readonly Adapter amqpAdapter;
        protected readonly string exchangeName;

        protected AMQPConsumerNotifier(Adapter amqpAdapter, string exchangeName) {
            this.amqpAdapter = amqpAdapter;
            this.exchangeName = exchangeName;
        }

        public abstract void Notify(ConcurrentBag<AMQPQueueMetric> busyQueues,
            ConcurrentBag<AMQPQueueMetric> quietQueues);
    }
}