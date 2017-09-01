using System;
using cAlgo.API;

namespace Niffler.Common.Trade
{
    class StopLossManager : IResetState
    {
        public bool IsBreakEvenStopLossActive { get; set; }
        private double HardStopLossBufferPips { get; set; }
        private double ResetHardStopLossBufferPips { get; set; }
        private double LastOrderStopLossPips { get; set; }
        private StateData BotState { get;  set; }
        private Robot Bot { get; set; }
        private ReportManager Reporter { get; set; }

        public StopLossManager(StateData s, double hardStopLossBufferPips, double lastOrderStopLossPips)
        {
            BotState = s;
            Bot = BotState.Bot;
            Reporter = BotState.GetReporter();
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
                        Bot.ModifyPositionAsync(p, stopLossPrice, p.TakeProfit, OnPositionSLOperationComplete);
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
                                Bot.ModifyPositionAsync(p, p.EntryPrice + SLBufferPips, p.TakeProfit, OnBreakEvenSLOperationComplete);
                            }
                        }

                        if (BotState.LastPositionTradeType == TradeType.Sell)
                        {
                            if (breakEvenTriggerPrice < p.EntryPrice)
                            {
                                Bot.ModifyPositionAsync(p, p.EntryPrice - SLBufferPips, p.TakeProfit, OnBreakEvenSLOperationComplete);
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

        protected void OnBreakEvenSLOperationComplete(TradeResult tr)
        {
            OnPositionOperationComplete(tr, "FAILED to set BreakEven StopLoss for position");
        }

        protected void OnPositionSLOperationComplete(TradeResult tr)
        {
            OnPositionOperationComplete(tr,"FAILED to UPDATE StopLoss for position");
        }

        protected void OnPositionOperationComplete(TradeResult tr, string errorMsg)
        {
            if (!tr.IsSuccessful)
            {
                if (tr.Position != null)
                {
                    Reporter.ReportTradeResultError(errorMsg + "," + tr.Position.Label + "," + tr.Position.TradeType + "," + System.DateTime.Now + "," + tr.Error);
                }
            }
        }
    }
}
