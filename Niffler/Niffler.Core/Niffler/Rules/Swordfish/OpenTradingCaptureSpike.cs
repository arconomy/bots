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
    class OpenTradingCaptureSpike : IRule
    {
        public OpenTradingCaptureSpike(int priority) : base(priority) {}

        // If Trading time then capture spike
        override protected void execute()
        {
            if (MarketInfo.IsBotTradingOpen())
            {
                SpikeManager.captureSpike();
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
