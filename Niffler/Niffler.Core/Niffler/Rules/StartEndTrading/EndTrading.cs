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
    class EndTrading : IRule
    {
        public EndTrading(int priority) : base(priority) { }

        //If rule should only execute when bot is trading return TRUE, default is FALSE
        protected override bool IsTradingRule()
        {
            return false;
        }

        //The IsTrading flag is the only state variable that is not reset by the rule that sets it
        override protected void Execute()
        {
            if (BotState.IsAfterTerminateTime || BotState.IsReset)
            {
                RulesManager.Reset();
                BotState.IsTrading = false;
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
