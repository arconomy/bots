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
    class RetracedLevel2To3SetHardSLToLastProfitPositionEntry : IRule
    {
        public RetracedLevel2To3SetHardSLToLastProfitPositionEntry(RulesManager rulesManager, int priority) : base(rulesManager, priority) { }

        //If Spike retrace is greater than Level 2 but less than Level 3 set SL to last profit position entry price
        override protected void execute()
        {
            //Calculate spike retrace factor
            SpikeManager.calculateRetraceFactor();

            if (SpikeManager.IsRetraceBetweenLevel2AndLevel3())
            {
                if (BotState.LastProfitPositionEntryPrice > 0)
                {
                    StopLossManager.setSLForAllPositions(BotState.LastProfitPositionEntryPrice);
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