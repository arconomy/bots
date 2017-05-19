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
    class SetBreakEvenSLPastLastProfitPositionEntry : IRule
    {
        public SetBreakEvenSLPastLastProfitPositionEntry(RulesManager rulesManager, int priority) : base(rulesManager, priority) { }

        //If BreakEven SL is Active then set BreakEven Stop Losses for all orders if the current price is past the entry point of the Last position to close with profit
        override protected void execute()
        {
            if (BotState.IsBreakEvenStopLossActive)
            {
                StopLossManager.setBreakEvenSLForAllPositions(BotState.LastProfitPositionEntryPrice, true);
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
