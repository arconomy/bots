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
    class ReduceRiskTimeSetTrailingStop : IRule
    {
        public ReduceRiskTimeSetTrailingStop(int priority) : base(priority) { }

        //If it is after reduce risk time then set the fixed trailing stop 
        override protected void execute()
        {
            if (BotState.IsAfterReducedRiskTime)
            {
                if (BotState.OrdersPlaced && BotState.positionsRemainOpen())
                {
                    FixedTrailingStop.activate();
                    ExecuteOnceOnly();
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
