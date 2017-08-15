using System;
using cAlgo.API;
using Google.Protobuf.Collections;

namespace Niffler.Rules
{
    class OnPositionClosedLastEntryPositionStopLossTriggeredCloseAll : IRuleOnPositionEvent
    {
        public OnPositionClosedLastEntryPositionStopLossTriggeredCloseAll(int priority) : base(priority) {}

        //If rule should only execute when bot is trading return TRUE, default is FALSE
        protected override bool IsTradingRule()
        {
            return true;
        }

        //Last position's SL has been triggered for a loss - CLOSING ALL POSITIONS
        override protected void Execute(Position position)
        {
            if (BotState.IsThisBotId(position.Label))
            {
                if (BotState.LastPositionLabel == position.Label && position.GrossProfit < 0)
                {
                    BuyLimitOrdersTrader.CancelAllPendingOrders();
                    PositionsManager.CloseAllPositions();
                    BotState.IsReset = true;
                    ExecuteOnceOnly();
                }

            }
        }

        // reset any botstate variables to the state prior to executing rule
        override protected void Reset()
        {
            BotState.IsReset = false;
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
