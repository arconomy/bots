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
    class OnPositionClosedInProfitCaptureProfitPositionInfo : IRuleOnPositionEvent
    {
        public OnPositionClosedInProfitCaptureProfitPositionInfo(int priority) : base(priority) {}

        //Report closing position trade
        override protected void execute(Position position)
        {
            if (BotState.isThisBotId(position.Label))
            {
                //Taking profit
                if (position.GrossProfit > 0)
                {
                    //capture last position take profit price
                    BotState.captureLastProfitPositionPrices(position);
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
