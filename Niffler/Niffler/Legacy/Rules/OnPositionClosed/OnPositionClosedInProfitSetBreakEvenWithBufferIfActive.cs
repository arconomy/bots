using System;
using cAlgo.API;
using Google.Protobuf.Collections;

namespace Niffler.Rules
{
    class OnPositionClosedInProfitSetBreakEvenWithBufferIfActive : IPositionRule
    {
        public OnPositionClosedInProfitSetBreakEvenWithBufferIfActive(int priority) : base(priority) { }

        //If rule should only execute when bot is trading return TRUE, default is FALSE
        protected override bool IsTradingRule()
        {
            return true;
        }

        //If BreakEven SL is active set breakeven SL + Buffer
        override protected void Execute(Position position)
        {
            if (BotState.IsThisBotId(position.Label))
            {
                if (StopLossManager.IsBreakEvenStopLossActive)
                {
                    StopLossManager.SetBreakEvenSLForAllPositions(BotState.LastProfitPositionEntryPrice, true);
                }
            }
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
