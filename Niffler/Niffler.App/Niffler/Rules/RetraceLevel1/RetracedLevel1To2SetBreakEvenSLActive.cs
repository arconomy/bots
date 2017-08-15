using System;
using Google.Protobuf.Collections;

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
        override protected bool Execute()
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
    }
}
