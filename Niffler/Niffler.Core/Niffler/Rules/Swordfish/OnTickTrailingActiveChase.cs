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
    class OnTickTrailingActiveChase : IRule
    {
        public OnTickTrailingActiveChase(int priority) : base(priority) {}

        //If the Trail is active then chase.
        override protected void execute()
        {
            FixedTrailingStop.chase();
        }

        override public void reportExecution()
        {
            // report stats on rule execution 
            // e.g. execution rate, last position rule applied to, number of positions impacted by rule
            // Gonna need some thought here.
        }
    }
}
