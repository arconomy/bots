using Niffler.Messaging.RabbitMQ;
using System.Collections.Generic;
using Niffler.Messaging.Protobuf;
using Niffler.Services;
using Niffler.Core.Config;
using Niffler.Rules.TradingPeriods;
using Niffler.Model;

namespace Niffler.Rules
{
    class Template : IRule
    {
        public Template(StrategyConfiguration StrategyConfig, RuleConfiguration ruleConfig) : base(StrategyConfig, ruleConfig) { }

        //Retreive data from config to initialise the rule
        public override void Init()
        {
            //Set Inactive if waiting for event or notification to activate
            SetActiveState(false);

            //Listen for state updates for this strategy
            StateManager.ListenForStateUpdates();
        }

        //Execute rule logic will only run if rule is activated
        override protected bool ExcuteRuleLogic(Niffle message)
        {
            //Test for the type of Trade message(s)
            if (IsTickMessageEmpty(message)) return false;
            if (IsOnPositionMessageEmpty(message)) return false;

            //Place the rule logic here..

            //Set inactive if only executing once
            SetActiveState(false);

            //return true to publish a success Service Notify message
            return true;
        }

        //Rule will receive Service notifications even if inactive
        override protected void OnServiceNotify(Niffle message, RoutingKey routingKey)
        {
            //Listening for Success Nofitification from a SOURCE service
            if (routingKey.Source == nameof(OnOpenForTrading) && message.Service.Success)
            {
                //Perform actions
                //Do not need to activate/deactivate as these rules are set in JSON config
            }
        }

        //Rule will receive Service notifications even if inactive
        protected override void OnStateUpdate(StateChangedEventArgs stateupdate)
        {
            //Listening for updates to State
        }

        protected override string GetServiceName()
        {
            //Return the name of this Service
            return nameof(Template);
        }

        protected override void AddListeningRoutingKeys(ref List<RoutingKey> routingKeys)
        {
            //Set the message(s) that this service is interested in receiving

            //e.g. Listen for OnTick
            routingKeys.Add(RoutingKey.Create(Source.WILDCARD, Messaging.RabbitMQ.Action.WILDCARD, Event.ONTICK));

            //e.g. Listen for Service Notification success messages from OnOpenForTrading
            routingKeys.Add(RoutingKey.Create(nameof(OnOpenForTrading), Messaging.RabbitMQ.Action.NOTIFY, Event.WILDCARD));
        }

        public override void Reset()
        {
            //Reset local variables and set to original ready state
            SetActiveState(false);
        }
    }
}
