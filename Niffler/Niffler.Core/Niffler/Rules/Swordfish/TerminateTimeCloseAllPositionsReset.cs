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
    class TerminateTimeCloseAllPositionsReset : IRule
    {
        public TerminateTimeCloseAllPositionsReset(RulesManager rulesManager, int priority) : base(rulesManager, priority) { }

        //If trades still open at Terminate Time then take the hit and close remaining positions
        override protected void execute()
        {
            if(BotState.IsAfterTerminateTime)
            {
                PositionsManager.closeAllPositions();
                RulesManager.reset();
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
