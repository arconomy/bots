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
    class OnPositionClosedInProfitSetBreakEvenWithBufferIfActive : IRuleOnPositionEvent
    {
        public OnPositionClosedInProfitSetBreakEvenWithBufferIfActive(int priority) : base(priority) {}

        //If BreakEven SL is active set breakeven SL + Buffer
        override protected void execute(Position position)
        {
            if (BotState.isThisBotId(position.Label))
            {
                if (StopLossManager.IsBreakEvenStopLossActive)
                {
                    StopLossManager.setBreakEvenSLForAllPositions(BotState.LastProfitPositionEntryPrice, true);
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
