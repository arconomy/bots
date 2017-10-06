using System;
using System.Collections.Generic;
using Google.Protobuf.Collections;
using Niffler.Messaging.Protobuf;
using Niffler.Messaging.RabbitMQ;

namespace Niffler.Rules
{
    class RetracedLevel1To2SetHardSLToLastProfitPositionEntryWithBuffer : IRule
    {
        public RetracedLevel1To2SetHardSLToLastProfitPositionEntryWithBuffer(int priority) : base(priority) { }

        //If rule should only execute when bot is trading return TRUE, default is FALSE
        protected override bool IsTradingRule()
        {
            return true;
        }

        //If Spike retrace is greater than Level 1 but less than Level 2 set SL to last profit position entry price plus buffer
        override protected bool ExcuteRuleLogic()
        {
            if (BotState.OrdersPlaced && BotState.PositionsRemainOpen())
            {
                //Calculate spike retrace factor
                SpikeManager.CalculateRetraceFactor();

                //Set hard stop losses and activate Trail if Spike has retraced between than retraceLevel1 and retraceLevel2
                if (SpikeManager.IsRetraceBetweenLevel1AndLevel2())
                {
                    //If Hard SL has not been set yet
                    if (BotState.LastProfitPositionEntryPrice > 0)
                    {
                        StopLossManager.SetSLWithBufferForAllPositions(BotState.LastProfitPositionEntryPrice);
                        return true;
                    }
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
