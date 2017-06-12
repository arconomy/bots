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
        public OnPositionClosedInProfitSetBreakEvenWithBufferIfActive(int priority) : base(priority) { }

        //If rule should only execute when bot is trading return TRUE, default is FALSE
        protected override bool IsTradingRule()
        {
            return true;
        }

        //If BreakEven SL is active set breakeven SL + Buffer
        override protected void Execute(Position position)
        {
            if (BotState.IsThisBotId(position.Label))
            {
                if (StopLossManager.IsBreakEvenStopLossActive)
                {
                    StopLossManager.SetBreakEvenSLForAllPositions(BotState.LastProfitPositionEntryPrice, true);
                }
            }
        }

        // reset any botstate variables to the state prior to executing rule
        override protected void Reset()
        {

        }

        // report stats on rule execution 
        // e.g. execution rate, last position rule applied to, number of positions impacted by rule
        override public void Report()
        {

        }
    }
}
