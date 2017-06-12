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
        public TerminateTimeCloseAllPositionsReset(int priority) : base(priority) { }

        //If rule should only execute when bot is trading return TRUE, default is FALSE
        protected override bool IsTradingRule()
        {
            return true;
        }

        //If trades still open at Terminate Time then take the hit and close remaining positions
        override protected void Execute()
        {
            if(BotState.IsAfterTerminateTime)
            {
                PositionsManager.CloseAllPositions();
                BotState.IsReset = true;
            }
        }

        // reset any botstate variables to the state prior to executing rule
        override protected void Reset()
        {
            BotState.IsReset = false;
        }

        // report stats on rule execution 
        // e.g. execution rate, last position rule applied to, number of positions impacted by rule
        override public void Report()
        {

        }
    }
}
