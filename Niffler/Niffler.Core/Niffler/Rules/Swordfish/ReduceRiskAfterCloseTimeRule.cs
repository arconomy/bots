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
    class ReduceRiskAfterCloseTimeRule : IRule
    {
        public ReduceRiskAfterCloseTimeRule(RulesManager rulesManager) : base(rulesManager) {}

        //Set hard stop losses as soon as Swordfish time is over
        override public void execute()
        {
           
            if (BotState.IsAfterCloseTime && !BotState.IsHardSLLastPositionEntryPrice)
            {
                StopLossManager.setSLWithBufferForAllPositions(BotState.LastPositionEntryPrice);
                BotState.IsHardSLLastPositionEntryPrice = true;
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
