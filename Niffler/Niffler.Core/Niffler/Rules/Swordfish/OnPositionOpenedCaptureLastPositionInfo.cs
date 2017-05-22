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
    class OnPositionOpenedCaptureLastPositionInfo : IRuleOnPositionEvent
    {
        public OnPositionOpenedCaptureLastPositionInfo(int priority) : base(priority) {}

        //Capture last Position Opened
        override protected void execute(Position position)
        {
            if (BotState.isThisBotId(position.Label))
            {
                BotState.OpenedPositionsCount++;
                BotState.LastPositionTradeType = position.TradeType;
                BotState.LastPositionEntryPrice = position.EntryPrice;
                BotState.LastPositionLabel = position.Label;
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
