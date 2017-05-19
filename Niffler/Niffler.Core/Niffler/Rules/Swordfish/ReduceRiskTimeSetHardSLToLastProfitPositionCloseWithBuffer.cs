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
    class ReduceRiskTimeSetHardSLToLastProfitPositionCloseWithBuffer : IRule
    {
        public ReduceRiskTimeSetHardSLToLastProfitPositionCloseWithBuffer(RulesManager rulesManager, int priority) : base(rulesManager, priority) { }

        //If after reduce risk time then set hard stop losses to Last Profit Positions Entry Price with buffer
        override protected void execute()
        {
            if (BotState.IsAfterReducedRiskTime)
            {
                //If Hard SL has not been set yet
                if (BotState.LastProfitPositionClosePrice > 0)
                {
                    StopLossManager.setSLWithBufferForAllPositions(BotState.LastProfitPositionClosePrice);
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
