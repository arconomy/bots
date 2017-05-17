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
    class CloseAllPendingOrdersRule : IRule
    {

        public bool execute(Robot Bot, State BotState)
        {
            // If it is after CloseTime then close all positions
            if (BotState.IsAfterCloseTime)
            {
                CloseAllPendingOrders();
                BotState.IsReset = true;
            }

            reportExecution();
            return true;
        }

        public void reportExecution()
        {
            //report stats on rule execution e.g. execution rate, last position rule applied to, number of positions impacted by rule - gonna need some thought here.
        }
    }
}
