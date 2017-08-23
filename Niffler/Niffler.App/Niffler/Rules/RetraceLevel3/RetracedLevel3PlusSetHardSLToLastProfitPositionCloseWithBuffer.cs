using System;
using Google.Protobuf.Collections;

namespace Niffler.Rules
{
    class RetracedLevel3PlusSetHardSLToLastProfitPositionCloseWithBuffer : IRule
    {
        public RetracedLevel3PlusSetHardSLToLastProfitPositionCloseWithBuffer(int priority) : base(priority) { }

        //If rule should only execute when bot is trading return TRUE, default is FALSE
        protected override bool IsTradingRule()
        {
            return true;
        }

        // If it is after CloseTime and remaining pending orders have not been closed then close all pending orders
        override protected bool ExcuteRuleLogic()
        {
            if (BotState.OrdersPlaced && BotState.PositionsRemainOpen())
            {
                //Calculate spike retrace factor
                SpikeManager.CalculateRetraceFactor();

                if (SpikeManager.IsRetraceGreaterThanLevel3())
                {
                    if (BotState.LastProfitPositionClosePrice > 0)
                    {
                        StopLossManager.SetSLWithBufferForAllPositions(BotState.LastProfitPositionClosePrice);
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
    }
}
