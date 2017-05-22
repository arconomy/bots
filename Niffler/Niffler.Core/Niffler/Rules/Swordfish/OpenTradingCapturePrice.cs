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
    class OpenTradingCapturePrice : IRule
    {
        public OpenTradingCapturePrice(int priority) : base(priority) { }

        //Get the Opening price for the trading period
        override protected void execute()
        {
            if (MarketInfo.IsBotTradingOpen())
            {
                BotState.OpenPrice = Bot.MarketSeries.Close.LastValue;
                BotState.OpenPriceCaptured = true;

                //Update Reset flag as ready to trade
                if (BotState.IsReset)
                    BotState.IsReset = false;

                ExecuteOnceOnly();
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
