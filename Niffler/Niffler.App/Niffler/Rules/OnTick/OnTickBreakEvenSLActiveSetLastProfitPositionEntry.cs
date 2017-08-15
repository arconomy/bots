using System;
using Google.Protobuf.Collections;

namespace Niffler.Rules
{
    class OnTickBreakEvenSLActiveSetLastProfitPositionEntry : IRule
    {
        public OnTickBreakEvenSLActiveSetLastProfitPositionEntry(int priority) : base(priority) { }

        //If rule should only execute when bot is trading return TRUE, default is FALSE
        protected override bool IsTradingRule()
        {
            return true;
        }

        //If BreakEven SL is Active then set BreakEven Stop Losses for all orders if the current price is past the entry point of the Last position to close with profit
        override protected bool Execute()
        {
            if (BotState.OrdersPlaced && BotState.PositionsRemainOpen())
            {
                if (StopLossManager.IsBreakEvenStopLossActive)
                {
                    StopLossManager.SetBreakEvenSLForAllPositions(BotState.LastProfitPositionEntryPrice, true);
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
