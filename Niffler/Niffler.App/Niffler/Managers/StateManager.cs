#region Includes

using System;
using System.Collections.Generic;
using Niffler.Messaging.RabbitMQ;
using RabbitMQ.Client;
using Niffler.Common;
using Niffler.Common.Market;

#endregion

namespace Niffler.Microservices {
    public class StateManager : IConsumer {

        private State State;
        private RoutingKey routingkey = new RoutingKey();
        private TimeInfo TradingTimeInfo;

        public StateManager(IDictionary<string, string> config) : base(config, GenerateBotId()) {}

        public override object Clone()
        {
            return new StateManager(BotConfig);
        }

        public override void Init()
        {
            //The queue name is the generated Bot Id
            State = new State(QueueName);
            //TradingTimeInfo = new TradingTimeInfo(Config);
        }

        static private string GenerateBotId()
        {
            Random randomIdGenerator = new Random();
            int id = randomIdGenerator.Next(0, 99999);
            return id.ToString("00000");
        }

        public override void MessageReceived(MessageReceivedEventArgs e)
        {
            throw new NotImplementedException();
        }

        protected override List<RoutingKey> GetListeningRoutingKeys()
        {
            RoutingKey routingKey = new RoutingKey();
            routingKey.SetAction("UpdateState");
            return routingKey.getRoutingKeyAsList();
        }
    }
}