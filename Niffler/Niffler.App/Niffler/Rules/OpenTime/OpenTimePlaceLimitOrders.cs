using System;
using Google.Protobuf.Collections;

namespace Niffler.Rules
{
    class OpenTimePlaceLimitOrders : IRule
    {
        public OpenTimePlaceLimitOrders(int priority) : base(priority) { }

        //If rule should only execute when bot is trading return TRUE, default is FALSE
        protected override bool IsTradingRule()
        {
            return true;
        }

        //Get the Opening price for the trading period
        override protected bool Execute()
        {
            if (BotState.IsOpenTime)
            {
                //Price moves TriggerOrderPlacementPips UP from open then look to set SELL LimitOrders
                if (BotState.OpenPrice + SellLimitOrdersTrader.EntryTriggerOrderPlacementPips < Bot.Symbol.Bid)
                {
                    SellLimitOrdersTrader.PlaceSellLimitOrders();
                    BotState.OrdersPlaced = true;
                    ExecuteOnceOnly();
                    return true;
                }
                //Price moves 5pts DOWN from open then look to set BUY LimitOrders
                else if (BotState.OpenPrice - BuyLimitOrdersTrader.EntryTriggerOrderPlacementPips > Bot.Symbol.Ask)
                {
                    BuyLimitOrdersTrader.PlaceBuyLimitOrders();
                    BotState.OrdersPlaced = true;
                    ExecuteOnceOnly();
                    return true;
                }
            }
            return false;
        }

        // reset any botstate variables to the state prior to executing rule
        override protected void Reset()
        {
            BotState.OrdersPlaced = false;
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
