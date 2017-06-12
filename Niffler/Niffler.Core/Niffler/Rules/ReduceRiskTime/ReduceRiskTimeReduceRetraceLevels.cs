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
    class ReduceRiskTimeReduceRetraceLevels : IRule
    {
        public ReduceRiskTimeReduceRetraceLevels(int priority) : base(priority) { }

        //If rule should only execute when bot is trading return TRUE, default is FALSE
        protected override bool IsTradingRule()
        {
            return true;
        }

        // If it is after Reduce Risk Time then reduce retrace levels by 50%
        override protected void Execute()
        {

            if (BotState.IsAfterReducedRiskTime)
            {
                if(BotState.OrdersPlaced && BotState.PositionsRemainOpen())
                {
                    //Reduce all retrace limits
                    SpikeManager.ReduceLevelsBy50Percent();
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
