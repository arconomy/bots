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
    class RetracedLevel1To2SetBreakEvenSLActive : IRule
    {
        public RetracedLevel1To2SetBreakEvenSLActive(RulesManager rulesManager, int priority) : base(rulesManager, priority) { }

        //Set BreakEven SL if Spike has retraced between than retraceLevel1 and retraceLevel2
        override protected void execute()
        {
            //Calculate spike retrace factor
            SpikeManager.calculateRetraceFactor();

            if (SpikeManager.IsRetraceBetweenLevel1AndLevel2())
            {
                BotState.IsBreakEvenStopLossActive = true;
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
