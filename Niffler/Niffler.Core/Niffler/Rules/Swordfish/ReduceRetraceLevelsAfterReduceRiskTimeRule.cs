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
    class ReduceRetraceLevelsAfterReduceRiskTime : IRule
    {
        public ReduceRetraceLevelsAfterReduceRiskTime(RulesManager rulesManager) : base(rulesManager) {}

        // If it is after CloseTime and remaining pending orders have not been closed then close all pending orders
        override public void execute()
        {

            if (BotState.IsAfterReducedRiskTime)
            {
                //reset HARD SL Limits with reduced SL's
                BotState.IsHardSLLastPositionEntryPrice = true;

                //Reduce all retrace limits
                SpikeManager.reduceLevelsBy50Percent();
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
