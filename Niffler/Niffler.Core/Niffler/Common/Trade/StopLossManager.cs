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
    class StopLossManager : IResetState
    {

        private bool IsBreakEvenStopsActive { get; set; }
        private double HardStopLossBufferPips { get; set; }
        private double ResetHardStopLossBufferPips { get; set; }
        private double LastOrderStopLossPips { get; set; }
        private State BotState { get;  set; }
        private Robot Bot { get; set; }

        public StopLossManager(State s, double hardStopLossBufferPips, double lastOrderStopLossPips)
        {
            BotState = s;
            Bot = BotState.Bot;
            IsBreakEvenStopsActive = false;
            HardStopLossBufferPips = hardStopLossBufferPips;
            ResetHardStopLossBufferPips = hardStopLossBufferPips;
            LastOrderStopLossPips = lastOrderStopLossPips;
        }

        public void reset()
        {
            HardStopLossBufferPips = ResetHardStopLossBufferPips;
        }

        public void setSLForAllPositions(double stopLossPrice)
        {
            foreach (Position p in Bot.Positions)
            {
                try
                {
                    if (BotState.isThisBotId(p.Label))
                    {
                        Bot.ModifyPositionAsync(p, stopLossPrice, p.TakeProfit, onTradeOperationComplete);
                    }
                }
                catch (Exception e)
                {
                    Bot.Print("FAILED to modify Stop Loss : " + e.Message);
                }
            }
        }

        public void reduceHardSLBufferBy50Percent()
        {
            HardStopLossBufferPips /= 2;
        }

        public void setSLWithBufferForAllPositions(double SLPrice)
        {
            switch (BotState.LastPositionTradeType)
            {
                case TradeType.Buy:
                    setSLForAllPositions(SLPrice - HardStopLossBufferPips);
                    break;
                case TradeType.Sell:
                    setSLForAllPositions(SLPrice + HardStopLossBufferPips);
                    break;
            }
        }

        public void setBreakEvenSLForAllPositions(double breakEvenTriggerPrice, bool withHardSLBuffer)
        {
            double SLBufferPips = 0;
            if(withHardSLBuffer)
            {
                SLBufferPips = HardStopLossBufferPips;
            }


            foreach (Position p in Bot.Positions)
            {
                try
                {
                    if (BotState.isThisBotId(p.Label))
                    {
                        if (BotState.LastPositionTradeType == TradeType.Buy)
                        {
                            if (breakEvenTriggerPrice > p.EntryPrice)
                            {
                                Bot.ModifyPositionAsync(p, p.EntryPrice + SLBufferPips, p.TakeProfit, onTradeOperationComplete);
                            }
                        }

                        if (BotState.LastPositionTradeType == TradeType.Sell)
                        {
                            if (breakEvenTriggerPrice < p.EntryPrice)
                            {
                                Bot.ModifyPositionAsync(p, p.EntryPrice - SLBufferPips, p.TakeProfit, onTradeOperationComplete);
                            }
                        }
                    }
                }
                catch (Exception e)
                {
                    Bot.Print("FAILED to modify BreakEven Stop Loss:" + e.Message);
                }
            }
        }



        protected void onTradeOperationComplete(TradeResult tr)
        {
            if (!tr.IsSuccessful)
            {
                string msg = "FAILED to update BreakEvenStop : " + tr.Error;
                if (tr.Position != null)
                    Bot.Print(msg, " Position: ", tr.Position.Label, " ", tr.Position.TradeType, " ", System.DateTime.Now);
                if (tr.PendingOrder != null)
                    Bot.Print(msg, " Pending Order: ", tr.PendingOrder.Label, " ", tr.PendingOrder.TradeType, " ", System.DateTime.Now);
            }
        }

    }
}
