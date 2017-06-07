﻿using System;
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

        //Report closing position trade
        override protected void execute(Position position)
        {
            if (BotState.IsThisBotId(position.Label))
            {
                BotState.ClosedPositionsCount++;
                Reporter.ReportTrade(position,StopLossManager.GetStopLossStatus());
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