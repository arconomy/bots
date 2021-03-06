﻿using Google.Protobuf;
using Niffler.Common;
using Niffler.Messaging.AMQP;
using Niffler.Messaging.Protobuf;
using RabbitMQ.Client;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Niffler.Messaging.RabbitMQ
{
    public class Publisher
   {

        private IModel Channel;
        private string ExchangeName;
        private IBasicProperties MessageProperties;

        public Publisher(IConnection connection, string exchangeName, IBasicProperties messageProperties = null, IDictionary<string, object> queueArgs = null)
        {
            Channel = connection.CreateModel();
            this.ExchangeName = exchangeName;
            MessageProperties = messageProperties;
            Channel.ExchangeDeclare(exchange: ExchangeName, type: Exchange.GetExchangeType(ExchangeType.TOPIC),durable: true,autoDelete: false, arguments: queueArgs);

        }

        public void UpdateState(State state, string entityName, string strategyId)
        {
            RoutingKey routingKey = RoutingKey.Create(entityName, Action.UPDATESTATE, Event.WILDCARD);

            Niffle niffle = new Niffle
            {
                StrategyId = strategyId,
                Type = Niffle.Types.Type.State,
                State = state
            };
            Publish(routingKey, niffle);
        }

        public void ServiceNotify(Service service, string entityName, string strategyId)
        {
            RoutingKey routingKey = RoutingKey.Create(entityName, Action.NOTIFY, Event.WILDCARD);

            Niffle niffle = new Niffle
            {
                StrategyId = strategyId,
                Timestamp = System.DateTime.Now.ToBinary(),
                Type = Niffle.Types.Type.Service,
                Service = service
            };
            Publish(routingKey, niffle);
        }

        public void TradeOperation(Trade trade, string entityName)
        {
            RoutingKey routingKey = RoutingKey.Create(entityName, Action.TRADEOPERATION, Event.WILDCARD);

            Niffle niffle = new Niffle
            {
                Type = Niffle.Types.Type.Trade,
                Trade = trade
            };
            Publish(routingKey, niffle);
        }

        private void Publish(RoutingKey routingKey,Niffle niffle)
        {
            Channel.BasicPublish(exchange: ExchangeName, routingKey: routingKey.GetRoutingKey(),
                basicProperties: RabbitMQProperties.CreateDefaultProperties(Channel), body: niffle.ToByteArray());
        }
    }
}
