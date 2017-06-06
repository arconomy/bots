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
    class OnTickBreakEvenSLActiveSetLastProfitPositionEntry : IRule
    {
        public OnTickBreakEvenSLActiveSetLastProfitPositionEntry(int priority) : base(priority) { }

        //If BreakEven SL is Active then set BreakEven Stop Losses for all orders if the current price is past the entry point of the Last position to close with profit
        override protected void Execute()
        {
            if (BotState.OrdersPlaced && BotState.PositionsRemainOpen())
            {
                if (StopLossManager.IsBreakEvenStopLossActive)
                {
                    StopLossManager.SetBreakEvenSLForAllPositions(BotState.LastProfitPositionEntryPrice, true);
                }
            }
        }

        override public void ReportExecution()
        {
            // report stats on rule execution 
            // e.g. execution rate, last position rule applied to, number of positions impacted by rule
            // Gonna need some thought here.
        }
    }
}
