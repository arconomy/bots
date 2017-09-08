using Niffler.Messaging.RabbitMQ;
using System.Collections.Generic;
using Niffler.Messaging.Protobuf;
using Niffler.Services;
using Niffler.Core.Config;
using Niffler.Rules.TradingPeriods;

namespace Niffler.Rules
{
    class Template : IRule
    {
        public Template(StrategyConfiguration StrategyConfig, RuleConfiguration ruleConfig) : base(StrategyConfig, ruleConfig) { }

        //Retreive data from config to initialise the rule
        public override void Init()
        {
            //Set Inactive if waiting for event or notification to activate
            IsActive = false;
        }

        //Execute rule logic
        override protected bool ExcuteRuleLogic(Niffle message)
        {
            //Test for the type of Trade message(s)
            if (IsTickMessageEmpty(message)) return false;
            if (IsOnPositionMessageEmpty(message)) return false;

            //Set inactive if only executing once
            IsActive = false;

            //return true to publish a success Service Notify message
            return true;
        }

        override protected void OnServiceNotify(Niffle message, RoutingKey routingKey)
        {
           

            //Listening for Success Nofitification from a SOURCE service
            if (routingKey.Source == nameof(OnOpenForTrading) && message.Service.Success)
            {
                //Perform actions e.g. Activate... IsActive = true;
            }
        }

        protected override void OnStateUpdate(Niffle message, RoutingKey routingKey)
        {
            //Test for State message(s)
            if (IsStateMessageEmpty(message)) return;

            //Listening for State change Nofitification from a StateManager (or other) service
            if (routingKey.Source == nameof(StateManager) && message.State.Key == "StateDataField")
            {
                //Perform actions e.g. update local variables
            }
        }

        protected override string GetServiceName()
        {
            //Return the name of this Service
            return nameof(Template);
        }

        protected override List<RoutingKey> SetListeningRoutingKeys()
        {
            //Set the message(s) that this service is interested in receiving

            //e.g. Listen for OnTick
            List<RoutingKey> routingKeys = RoutingKey.Create(Source.WILDCARD, Messaging.RabbitMQ.Action.WILDCARD, Event.ONTICK).ToList();

            //e.g. Listen for Service Notification success messages from OnOpenForTrading
            routingKeys.Add(RoutingKey.Create(nameof(OnOpenForTrading), Messaging.RabbitMQ.Action.NOTIFY, Event.WILDCARD));

            return routingKeys;
        }

        public override void Reset()
        {
            //Reset local variables and set to original ready state
            IsActive = false;
        }
    }
}
