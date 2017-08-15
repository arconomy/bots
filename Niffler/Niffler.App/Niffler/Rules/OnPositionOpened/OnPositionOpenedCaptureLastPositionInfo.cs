using System;
using cAlgo.API;
using Google.Protobuf.Collections;

namespace Niffler.Rules
{
    class OnPositionOpenedCaptureLastPositionInfo : IRuleOnPositionEvent
    {
        public OnPositionOpenedCaptureLastPositionInfo(int priority) : base(priority) {}

        //If rule should only execute when bot is trading return TRUE, default is FALSE
        protected override bool IsTradingRule()
        {
            return true;
        }

        //Capture last Position Opened
        override protected void Execute(Position position)
        {
            if (BotState.IsThisBotId(position.Label))
            {
                BotState.OpenedPositionsCount++;
                BotState.LastPositionTradeType = position.TradeType;
                BotState.LastPositionEntryPrice = position.EntryPrice;
                BotState.LastPositionLabel = position.Label;
            }
        }

        // reset any botstate variables to the state prior to executing rule
        override protected void Reset()
        {
            BotState.LastPositionEntryPrice = 0;
            BotState.LastPositionLabel = "NO LAST POSITION SET";
            BotState.OpenedPositionsCount = 0;
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
