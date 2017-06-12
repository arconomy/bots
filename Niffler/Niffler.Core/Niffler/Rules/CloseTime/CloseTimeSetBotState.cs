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
    class CloseTimeSetBotState : IRule
    {
        public CloseTimeSetBotState(int priority) : base(priority) { }

        //If rule should only execute when bot is trading return TRUE, default is FALSE
        protected override bool IsTradingRule()
        {
            return true;  //Close time is only interesting if the bot is trading
        }

        //After CLose time set hard stop losses at last position entry price with Buffer
        override protected void Execute()
        {
            if (!BotState.IsAfterCloseTime && MarketInfo.IsAfterCloseTime())
            {
                BotState.IsAfterCloseTime = true;
                ExecuteOnceOnly();
            }
                
        }

        override protected void Reset()
        {
            BotState.IsAfterCloseTime = false;
        }

        override public void Report()
        {
            // report stats on rule execution 
            // e.g. execution rate, last position rule applied to, number of positions impacted by rule
            // Gonna need some thought here.
        }
    }
}
