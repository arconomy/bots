using System;
using System.Collections.Generic;
using Google.Protobuf.Collections;
using Niffler.Messaging.Protobuf;
using Niffler.Messaging.RabbitMQ;

namespace Niffler.Rules
{
    class RetracedLevel1To2SetBreakEvenSLActive : IRule
    {
        public RetracedLevel1To2SetBreakEvenSLActive(int priority) : base(priority) { }

        //If rule should only execute when bot is trading return TRUE, default is FALSE
        protected override bool IsTradingRule()
        {
            return true;
        }

        //Set BreakEven SL if Spike has retraced between than retraceLevel1 and retraceLevel2
        override protected bool ExcuteRuleLogic()
        {
            if (BotState.OrdersPlaced && BotState.PositionsRemainOpen())
            {
                //Calculate spike retrace factor
                SpikeManager.CalculateRetraceFactor();

                if (SpikeManager.IsRetraceBetweenLevel1AndLevel2())
                {
                    StopLossManager.IsBreakEvenStopLossActive = true;
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

        public bool IsRetraceBetweenLevel1AndLevel2()
        {
            return Level1 < RetraceFactor && RetraceFactor < Level2;
        }

        public bool IsRetraceBetweenLevel2AndLevel3()
        {
            return Level2 < RetraceFactor && RetraceFactor < Level3;
        }

        public bool IsRetraceGreaterThanLevel3()
        {
            return Level3 < RetraceFactor;
        }

        public bool IsRetraceLessThanLevel1()
        {
            return Level1 > RetraceFactor;
        }

        public void ReduceLevelsBy50Percent()
        {
            Level1 /= 2;
            Level2 /= 2;
            Level3 /= 2;
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
