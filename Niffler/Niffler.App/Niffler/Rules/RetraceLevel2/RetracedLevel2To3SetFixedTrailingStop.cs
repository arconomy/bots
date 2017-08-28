using System;
using Niffler.Common.TrailingStop;
using Google.Protobuf.Collections;
using Niffler.Messaging.Protobuf;
using Niffler.Messaging.RabbitMQ;
using System.Collections.Generic;

namespace Niffler.Rules
{
    class RetracedLevel2To3SetFixedTrailingStop : IRule
    {
        public RetracedLevel2To3SetFixedTrailingStop(int priority) : base(priority) { }

        //If rule should only execute when bot is trading return TRUE, default is FALSE
        protected override bool IsTradingRule()
        {
            return true;
        }

        //If Spike retrace is greater than Level 2 but less than Level 3 set Fixed Trailing Stop
        override protected bool ExcuteRuleLogic()
        {
            if (BotState.OrdersPlaced && BotState.PositionsRemainOpen())
            {
                if (SpikeManager.IsRetraceBetweenLevel2AndLevel3())
                {
                    //Activate Trailing Stop Losses
                    FixedTrailingStop.activate();
                    ExecuteOnceOnly();
                    return true;
                }
            }
            return false;
        }

        // reset any botstate variables to the state prior to executing rule
        override protected void Reset()
        {

        }

        // report stats on rule execution 
        // e.g. execution rate, last position rule applied to, number of positions impacted by rule
        override public MapField<String, String> GetLastExecutionData()
        {
            return new MapField<string, string> { { "result", "success" } };

        }

        //create name of Rule Topic for Pub/Sub
        public override string GetPubSubTopicName()
        {
            return this.GetType().Name;
        }

        protected override string GetServiceName()
        {
            throw new NotImplementedException();
        }

        protected override bool ExcuteRuleLogic(Niffle message)
        {
            throw new NotImplementedException();
        }

        protected override List<RoutingKey> SetListeningRoutingKeys()
        {
            throw new NotImplementedException();
        }

        protected override void OnServiceNotify(Niffle message, RoutingKey routingKey)
        {
            throw new NotImplementedException();
        }

        protected override void OnStateUpdate(Niffle message, RoutingKey routingKey)
        {
            throw new NotImplementedException();
        }

        public override object Clone()
        {
            throw new NotImplementedException();
        }

        public override bool Init()
        {
            throw new NotImplementedException();
        }
    }
}
