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
    class CloseTimeCancelPendingOrders : IRule
    {
        public CloseTimeCancelPendingOrders(RulesManager rulesManager, int priority) : base(rulesManager,priority) {}

        // If it is after CloseTime and remaining pending orders have not been closed then close all pending orders
        override protected void execute()
        {
            if (!BotState.IsPendingOrdersClosed && BotState.IsAfterCloseTime)
            {
                OrdersManager.closeAllPendingOrders();
                ExecuteOnceOnly();
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