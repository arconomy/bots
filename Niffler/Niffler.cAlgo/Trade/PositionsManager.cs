using System;
using cAlgo.API;

namespace Niffler.Common.Trade
{
    class PositionsManager
    { 
        private StateData BotState { get; set; }
        private Robot Bot { get; set; }
        private ReportManager Reporter { get;  set; } 

        public PositionsManager(StateData s)
        {
            BotState = s;
            Bot = BotState.Bot;
            Reporter = BotState.GetReporter();
        }

        public void CloseAllPositions()
        {
            //Close any outstanding pending orders
            foreach (Position p in Bot.Positions)
            {
                try
                {
                    if (BotState.IsThisBotId(p.Label))
                    {
                        Bot.ClosePositionAsync(p, OnPositionCloseOperationComplete);
                    }
                }
                catch (Exception e)
                {
                    Bot.Print("Failed to Close Position: " + e.Message);
                }
            }
            BotState.IsOpenTime = true;
        }

        protected void OnPositionCloseOperationComplete(TradeResult tr)
        {
            if (!tr.IsSuccessful)
            {
                if(tr.Position != null)
                {
                    Reporter.ReportTradeResultError("FAILED to CLOSE position," + tr.Position.Label + "," + tr.Position.TradeType + "," + System.DateTime.Now + "," + tr.Error);
                }
            }
        }


    }
}
