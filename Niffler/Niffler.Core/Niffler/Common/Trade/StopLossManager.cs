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
        public bool IsBreakEvenStopLossActive { get; set; }
        private double HardStopLossBufferPips { get; set; }
        private double ResetHardStopLossBufferPips { get; set; }
        private double LastOrderStopLossPips { get; set; }
        private State BotState { get;  set; }
        private Robot Bot { get; set; }

        public StopLossManager(State s, double hardStopLossBufferPips, double lastOrderStopLossPips)
        {
            BotState = s;
            Bot = BotState.Bot;
            IsBreakEvenStopLossActive = false;
            HardStopLossBufferPips = hardStopLossBufferPips;
            ResetHardStopLossBufferPips = hardStopLossBufferPips;
            LastOrderStopLossPips = lastOrderStopLossPips;
        }

        public String GetStopLossStatus()
        {
            return "," + IsBreakEvenStopLossActive;
        }

        public String GetStopLossStatusHeaders()
        {
            return ",IsBreakEvenStopLossActive";
        }

        public void Reset()
        {
            HardStopLossBufferPips = ResetHardStopLossBufferPips;
        }

        public void SetSLForAllPositions(double stopLossPrice)
        {
            foreach (Position p in Bot.Positions)
            {
                try
                {
                    if (BotState.IsThisBotId(p.Label))
                    {
                        Bot.ModifyPositionAsync(p, stopLossPrice, p.TakeProfit, OnTradeOperationComplete);
                    }
                }
                catch (Exception e)
                {
                    Bot.Print("FAILED to modify Stop Loss : " + e.Message);
                }
            }
        }

        public void ReduceHardSLBufferBy50Percent()
        {
            HardStopLossBufferPips /= 2;
        }

        public void SetSLWithBufferForAllPositions(double SLPrice)
        {
            switch (BotState.LastPositionTradeType)
            {
                case TradeType.Buy:
                    SetSLForAllPositions(SLPrice - HardStopLossBufferPips);
                    break;
                case TradeType.Sell:
                    SetSLForAllPositions(SLPrice + HardStopLossBufferPips);
                    break;
            }
        }

        public void SetBreakEvenSLForAllPositions(double breakEvenTriggerPrice, bool withHardSLBuffer)
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
                    if (BotState.IsThisBotId(p.Label))
                    {
                        if (BotState.LastPositionTradeType == TradeType.Buy)
                        {
                            if (breakEvenTriggerPrice > p.EntryPrice)
                            {
                                Bot.ModifyPositionAsync(p, p.EntryPrice + SLBufferPips, p.TakeProfit, OnTradeOperationComplete);
                            }
                        }

                        if (BotState.LastPositionTradeType == TradeType.Sell)
                        {
                            if (breakEvenTriggerPrice < p.EntryPrice)
                            {
                                Bot.ModifyPositionAsync(p, p.EntryPrice - SLBufferPips, p.TakeProfit, OnTradeOperationComplete);
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



        protected void OnTradeOperationComplete(TradeResult tr)
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
