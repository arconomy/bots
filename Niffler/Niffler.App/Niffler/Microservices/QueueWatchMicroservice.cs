﻿#region Includes

using System;
using System.Linq;
using System.Threading;
using Niffler.Messaging.RabbitMQ;
using Niffler.Messaging.AMQP;

#endregion

namespace Niffler.Microservices {
    public class QueueWatchMicroservice : IService {
        private QueueWatch _queueWatch;

        public void Init() {

            var amqpQueueMetricsManager = new RabbitMQQueueMetricsManager(false, "localhost", 15672, "paul", "password");
            AMQPQueueMetricsAnalyser amqpQueueMetricsAnalyser = new RabbitMQQueueMetricsAnalyser(
                new ConsumerUtilisationTooLowAMQPQueueMetricAnalyser(
                    new ConsumptionRateIncreasedAMQPQueueMetricAnalyser(
                        new DispatchRateDecreasedAMQPQueueMetricAnalyser(
                            new QueueLengthIncreasedAMQPQueueMetricAnalyser(
                                new ConsumptionRateDecreasedAMQPQueueMetricAnalyser(
                                    new StableAMQPQueueMetricAnalyser()))))), 20);
            AMQPConsumerNotifier amqpConsumerNotifier = new RabbitMQConsumerNotifier(Adapter.Instance, "monitor");
            Adapter.Instance.Init();
            _queueWatch = new QueueWatch(amqpQueueMetricsManager, amqpQueueMetricsAnalyser, amqpConsumerNotifier, 5000);
            _queueWatch.AMQPQueueMetricsAnalysed += QueueWatchOnAMQPQueueMetricsAnalysed;

            _queueWatch.StartAsync();
        }

        private void QueueWatchOnAMQPQueueMetricsAnalysed(object sender, AMQPQueueMetricsAnalysedEventArgs e) {

            Console.Clear();
            if (e.BusyQueues.Any()) {
                foreach (var busy in e.BusyQueues)
                    Console.WriteLine(string.Concat(busy.QueueName, ": ", busy.AMQPQueueMetricAnalysisResult));
            }
            if (!e.QuietQueues.Any()) return;
            foreach (var quiet in e.QuietQueues)
                Console.WriteLine(string.Concat(quiet.QueueName, ": ", quiet.AMQPQueueMetricAnalysisResult));

            Console.WriteLine("-----");

            ThreadPool.GetAvailableThreads(out int workerThreads, out int ioThreads);
            Console.WriteLine(string.Concat("Worker Threads: ", workerThreads));
            Console.WriteLine(string.Concat("IO Threads: ", ioThreads));

            Console.WriteLine("-----");
        }

        public void OnMessageReceived(object sender, MessageReceivedEventArgs e) {
            throw new NotImplementedException();
        }

        public void Stop() {

            if (_queueWatch != null) {
                _queueWatch.Stop();
            }
        }
    }
}