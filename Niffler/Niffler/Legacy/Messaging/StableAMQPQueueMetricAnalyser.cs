#region Includes

using System.Collections.Concurrent;

#endregion

namespace Niffler.Messaging.AMQP {
    public class StableAMQPQueueMetricAnalyser : AMQPQueueMetricAnalyser {
        public StableAMQPQueueMetricAnalyser() : base(null) {}

        public override void Analyse(AMQPQueueMetric current, AMQPQueueMetric previous, ConcurrentBag<AMQPQueueMetric> busyQueues, ConcurrentBag<AMQPQueueMetric> quietQueues, int percentageDifference) {
            quietQueues.Add(current);
            current.AMQPQueueMetricAnalysisResult = AMQPQueueMetricAnalysisResult.Stable;
        }
    }
}