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

        public PositionsManager(Robot r, State s)
        {
            Bot = r;
            BotState = s;
        }

        protected void CloseAllPositions()
        {
            //Close any outstanding pending orders
            foreach (Position p in Positions)
            {
                try
                {
                    if (isThisBotId(p.Label))
                    {
                        ClosePositionAsync(p, onTradeOperationComplete);
                    }
                }
                catch (Exception e)
                {
                    Print("Failed to Close Position: " + e.Message);
                }
            }
        }


    }
}
