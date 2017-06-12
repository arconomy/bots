using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using cAlgo.API;
using cAlgo.API.Indicators;
using cAlgo.API.Internals;
using Niffler.Common;

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
        override protected void Execute()
        {
            if (!BotState.IsPendingOrdersClosed && BotState.IsAfterCloseTime)
            {
                SellLimitOrdersTrader.CancelAllPendingOrders();
                BotState.IsPendingOrdersClosed = true;
                ExecuteOnceOnly();
            }
        }

        override protected void Reset()
        {
            BotState.IsPendingOrdersClosed = false;
        }

        // report stats on rule execution 
        // e.g. execution rate, last position rule applied to, number of positions impacted by rule
        override public void Report()
        {
            
        }
    }
}
