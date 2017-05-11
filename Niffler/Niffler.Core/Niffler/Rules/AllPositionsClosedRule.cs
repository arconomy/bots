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
    class AllPositionsClosedRule : IRule
    {

        public bool execute(Robot Bot, State BotState)
        {
            // Execute the rule logic
            if (Bot.Positions.Count < 0)
            {
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
