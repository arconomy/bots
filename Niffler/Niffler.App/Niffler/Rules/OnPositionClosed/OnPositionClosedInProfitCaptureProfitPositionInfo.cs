using System;
using cAlgo.API;
using Google.Protobuf.Collections;

namespace Niffler.Rules
{
    class OnPositionClosedInProfitCaptureProfitPositionInfo : IRuleOnPositionEvent
    {
        public OnPositionClosedInProfitCaptureProfitPositionInfo(int priority) : base(priority) {}

        //If rule should only execute when bot is trading return TRUE, default is FALSE
        protected override bool IsTradingRule()
        {
            return true;
        }

        //Report closing position trade
        override protected void Execute(Position position)
        {
            if (BotState.IsThisBotId(position.Label))
            {
                //Taking profit
                if (position.GrossProfit > 0)
                {
                    //capture last position take profit price
                    BotState.LastProfitPositionClosePrice = Bot.History.FindLast(position.Label, Bot.Symbol, position.TradeType).ClosingPrice;
                    BotState.LastProfitPositionEntryPrice = position.EntryPrice;
                }
            }
        }

        // reset any botstate variables to the state prior to executing rule
        override protected void Reset()
        {
            BotState.LastProfitPositionEntryPrice = 0;
            BotState.LastProfitPositionClosePrice = 0;
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
