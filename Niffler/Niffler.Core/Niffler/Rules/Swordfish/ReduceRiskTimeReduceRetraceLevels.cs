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
    class ReduceRiskTimeReduceRetraceLevels : IRule
    {
        public ReduceRiskTimeReduceRetraceLevels(RulesManager rulesManager, int priority) : base(rulesManager, priority) { }

        // If it is after Reduce Risk Time then reduce retrace levels by 50%
        override protected void execute()
        {

            if (BotState.IsAfterReducedRiskTime)
            {
                //Reduce all retrace limits
                SpikeManager.reduceLevelsBy50Percent();
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
