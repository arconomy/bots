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
    class OnPositionClosedReportTrade : IRuleOnPositionEvent
    {
        public OnPositionClosedReportTrade(int priority) : base(priority) {}

        //If rule should only execute when bot is trading return TRUE, default is FALSE
        protected override bool IsTradingRule()
        {
            return true;
        }

        //Report closing position trade
        override protected void Execute(Position position)
        {
            if (BotState.IsThisBotId(position.Label))
            {
                BotState.ClosedPositionsCount++;
                Reporter.ReportTrade(position,StopLossManager.GetStopLossStatus());
            }
        }

        // reset any botstate variables to the state prior to executing rule
        override protected void Reset()
        {
            BotState.ClosedPositionsCount = 0;
        }

        // report stats on rule execution 
        // e.g. execution rate, last position rule applied to, number of positions impacted by rule
        override public void Report()
        {

        }
    }
}
