using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using cAlgo.API;
using cAlgo.API.Indicators;
using cAlgo.API.Internals;
using Niffler.Common;
using Niffler.Common.TrailingStop;

namespace Niffler.Rules
{
    class RetracedLevel2To3SetFixedTrailingStop : IRule
    {
        public RetracedLevel2To3SetFixedTrailingStop(int priority) : base(priority) { }

        //If rule should only execute when bot is trading return TRUE, default is FALSE
        protected override bool IsTradingRule()
        {
            return true;
        }

        //If Spike retrace is greater than Level 2 but less than Level 3 set Fixed Trailing Stop
        override protected void Execute()
        {
            if (BotState.OrdersPlaced && BotState.PositionsRemainOpen())
            {
                if (SpikeManager.IsRetraceBetweenLevel2AndLevel3())
                {
                    //Activate Trailing Stop Losses
                    FixedTrailingStop.activate();
                    ExecuteOnceOnly();
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
