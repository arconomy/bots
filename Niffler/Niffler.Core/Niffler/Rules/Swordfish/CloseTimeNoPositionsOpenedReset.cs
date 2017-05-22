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
    class CloseTimeNoPositionsOpenedReset : IRule
    {
        public CloseTimeNoPositionsOpenedReset(int priority) : base(priority) {}

        // If it is after CloseTime and remaining pending orders have not been closed then close all pending orders
        override protected void execute()
        {
            if (BotState.IsPendingOrdersClosed && BotState.positionsNotOpened() && BotState.IsAfterCloseTime)
            {
                RulesManager.reset();
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
