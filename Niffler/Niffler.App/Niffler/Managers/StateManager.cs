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
        private string StrategyId;


        public StateManager(IDictionary<string, string> config) : base(config)
        {
            config.TryGetValue("StrategyId", out string StrategyId));
        }

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