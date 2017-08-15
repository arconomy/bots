using System;
using Google.Protobuf.Collections;

namespace Niffler.Rules
{
    class CloseTimeCancelPendingOrders : IRule
    {
        public CloseTimeCancelPendingOrders(int priority) : base(priority) { }

        //If rule should only execute when bot is trading return TRUE, default is FALSE
        protected override bool IsTradingRule()
        {
            return true;
        }

        //If it is after CloseTime and remaining pending orders have not been closed then close all pending orders
        override protected bool Execute()
        {
            if (!BotState.IsPendingOrdersClosed && BotState.IsAfterCloseTime)
            {
                SellLimitOrdersTrader.CancelAllPendingOrders();
                BotState.IsPendingOrdersClosed = true;
                ExecuteOnceOnly();
                return true;
            }
            return false;
        }

        override protected void Reset()
        {
            BotState.IsPendingOrdersClosed = false;
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
