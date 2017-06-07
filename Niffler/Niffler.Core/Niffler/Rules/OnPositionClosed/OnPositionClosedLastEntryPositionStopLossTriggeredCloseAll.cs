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
    class OnPositionClosedLastEntryPositionStopLossTriggeredCloseAll : IRuleOnPositionEvent
    {
        public OnPositionClosedLastEntryPositionStopLossTriggeredCloseAll(int priority) : base(priority) {}

        //Last position's SL has been triggered for a loss - CLOSING ALL POSITIONS
        override protected void execute(Position position)
        {
            if (BotState.IsThisBotId(position.Label))
            {
                if (BotState.LastPositionLabel == position.Label && position.GrossProfit < 0)
                {
                    BuyLimitOrdersTrader.CancelAllPendingOrders();
                    PositionsManager.CloseAllPositions();
                    BotState.IsTerminated = true;
                    ExecuteOnceOnly();
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
