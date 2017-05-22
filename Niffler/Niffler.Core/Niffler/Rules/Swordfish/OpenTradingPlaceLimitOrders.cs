﻿using System;
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
    class OpenTradingPlaceLimitOrders : IRule
    {
        public OpenTradingPlaceLimitOrders(int priority) : base(priority) { }

        //Get the Opening price for the trading period
        override protected void execute()
        {
            if (MarketInfo.IsBotTradingOpen())
            {
                //Price moves TriggerOrderPlacementPips UP from open then look to set SELL LimitOrders
                if (BotState.OpenPrice + SellLimitOrdersTrader.EntryTriggerOrderPlacementPips < Bot.Symbol.Bid)
                {
                    SellLimitOrdersTrader.placeSellLimitOrders();
                    ExecuteOnceOnly();
                }
                //Price moves 5pts DOWN from open then look to set BUY LimitOrders
                else if (BotState.OpenPrice - BuyLimitOrdersTrader.EntryTriggerOrderPlacementPips > Bot.Symbol.Ask)
                {
                    BuyLimitOrdersTrader.placeBuyLimitOrders();
                    ExecuteOnceOnly();
                }
            }
        }

        override public void reportExecution()
        {
            // report stats on rule execution 
            // e.g. execution rate, last position rule applied to, number of positions impacted by rule
            // Gonna need some thought here.
        }
    }
}
