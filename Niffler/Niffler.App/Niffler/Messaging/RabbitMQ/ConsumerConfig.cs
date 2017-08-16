using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Niffler.Messaging.RabbitMQ
{
    public class ConsumerConfig
    {
        public string ExchangeName { get; set; }
        public string ExchangeType { get; set; }
        public string QueueName { get; set; }
        public string[] RoutingKeys { get; set; }


        public ConsumerConfig(string exchangeName, string exchangeType, string queueName, string[] routingKeys)
        {
            this.ExchangeName = exchangeName;
            this.ExchangeType = exchangeType;
            this.QueueName = queueName;
            this.RoutingKeys = routingKeys;
        }
    }
}
