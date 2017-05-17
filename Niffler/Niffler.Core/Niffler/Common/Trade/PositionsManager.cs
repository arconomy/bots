using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using cAlgo;
using cAlgo.API;
using cAlgo.API.Internals;
using cAlgo.API.Indicators;

namespace Niffler.Common.Trade
{
    class PositionsManager
    { 
        private State BotState { get; set; }
        private Robot Bot { get; set; }

        public PositionsManager(State s)
        {
            BotState = s;
            Bot = BotState.Bot;
        }

        public void closeAllPositions()
        {
            //Close any outstanding pending orders
            foreach (Position p in Bot.Positions)
            {
                try
                {
                    if (BotState.isThisBotId(p.Label))
                    {
                        Bot.ClosePositionAsync(p, onTradeOperationComplete);
                    }
                }
                catch (Exception e)
                {
                    Bot.Print("Failed to Close Position: " + e.Message);
                }
            }
            BotState.IsTerminated = true;
        }

        protected void onTradeOperationComplete(TradeResult tr)
        {
            if (!tr.IsSuccessful)
            {
                string msg = "FAILED Trade Operation for Position: " + tr.Error;
                Bot.Print(msg, " Pending Order: ", tr.Position.Label, " ", tr.Position.TradeType, " ", System.DateTime.Now);
            }
        }


    }
}
