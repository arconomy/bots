using Google.Protobuf;
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

        public Publisher(IConnection connection, string exchangeName)
        {
            Channel = connection.CreateModel();
            this.ExchangeName = exchangeName;
            Channel.ExchangeDeclare(exchange: ExchangeName, type: Exchange.GetExchangeType(ExchangeType.TOPIC));
        }

        public void UpdateState(State state, string entityName)
        {
            RoutingKey routingKey = RoutingKey.Create(entityName, Action.UPDATESTATE, Event.WILDCARD);

            Niffle niffle = new Niffle
            {
                Type = Niffle.Types.Type.State,
                State = state
            };
            ByteString body = niffle.ToByteString();

            Channel.BasicPublish(exchange: ExchangeName, routingKey: routingKey.GetRoutingKey(), basicProperties: null, body: body.ToByteArray());
        }

        public void ServiceNotify(Service service, string entityName)
        {
            RoutingKey routingKey = RoutingKey.Create(entityName, Action.NOTIFY, Event.WILDCARD);

            Niffle niffle = new Niffle
            {
                Type = Niffle.Types.Type.Service,
                Service = service
            };
            ByteString body = niffle.ToByteString();

            Channel.BasicPublish(exchange: ExchangeName, routingKey: routingKey.GetRoutingKey(), basicProperties: null, body: body.ToByteArray());
        }

        public void TradeOperation(Trade trade, string entityName)
        {
            RoutingKey routingKey = RoutingKey.Create(entityName, Action.TRADEOPERATION, Event.WILDCARD);

            Niffle niffle = new Niffle
            {
                Type = Niffle.Types.Type.Trade,
                Trade = trade
            };
            ByteString body = niffle.ToByteString();

            Channel.BasicPublish(exchange: ExchangeName, routingKey: routingKey.GetRoutingKey(), basicProperties: null, body: body.ToByteArray());
        }
    }
}
