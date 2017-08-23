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

        public void UpdateState(State state)
        {
            RoutingKey routingKey = new RoutingKey();
            routingKey.SetAction(Action.UPDATESTATE);

            Niffle niffle = new Niffle
            {
                Type = Niffle.Types.Type.Updatestate,
                State = state
            };
            ByteString body = niffle.ToByteString();

            Channel.BasicPublish(exchange: ExchangeName, routingKey: routingKey.GetRoutingKey(), basicProperties: null, body: body.ToByteArray());
        }

        public void ServiceNotify(Service service, RoutingKey routingKey)
        {
            routingKey.SetAction(Action.NOTIFY);

            Niffle niffle = new Niffle
            {
                Type = Niffle.Types.Type.Updateservice,
                Service = service
            };
            ByteString body = niffle.ToByteString();

            Channel.BasicPublish(exchange: ExchangeName, routingKey: routingKey.GetRoutingKey(), basicProperties: null, body: body.ToByteArray());
        }
    }
}
