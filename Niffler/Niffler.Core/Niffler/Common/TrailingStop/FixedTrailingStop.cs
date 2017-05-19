using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using cAlgo;
using cAlgo.API;
using cAlgo.API.Internals;
using cAlgo.API.Indicators;

namespace Niffler.Common.TrailingStop
{
    class FixedTrailingStop : IResetState
    {

        private bool IsActive { get; set; }
        private double TrailingStopPips { get; set; }
        private double ResetTrailingStopPips { get; set; }
        private State BotState { get; set; }
        private Robot Bot { get; set; }

        public FixedTrailingStop(State s, double trailingStopPips)
        {
            Bot = BotState.Bot;
            BotState = s;
            TrailingStopPips = trailingStopPips;
            ResetTrailingStopPips = trailingStopPips;
            IsActive = false;
        }

        public void reset()
        {
            IsActive = false;
            TrailingStopPips = ResetTrailingStopPips;
        }

        public void activate()
        {
            IsActive = true;
        }

        // If Trailing stop is active update position SL's - Remove TP as trailing position.
        public void chase()
        {
            if (IsActive)
            {
                foreach (Position p in Bot.Positions)
                {
                    try
                    {
                        if (BotState.isThisBotId(p.Label))
                        {

                            double newStopLossPrice = calcTrailingStopLoss(p);
                            if (newStopLossPrice > 0)
                            {
                                Bot.ModifyPositionAsync(p, newStopLossPrice, null, onTradeOperationComplete);
                            }
                        }
                    }
                    catch (Exception e)
                    {
                        Bot.Print("Failed to Modify Position:" + e.Message);
                    }
                }
            }
        }

        protected void onTradeOperationComplete(TradeResult tr)
        {
            if (!tr.IsSuccessful)
            {
                string msg = "FAILED to update TrailingStop : " + tr.Error;
                if (tr.Position != null)
                    Bot.Print(msg, " Position: ", tr.Position.Label, " ", tr.Position.TradeType, " ", System.DateTime.Now);
                if (tr.PendingOrder != null)
                    Bot.Print(msg, " Pending Order: ", tr.PendingOrder.Label, " ", tr.PendingOrder.TradeType, " ", System.DateTime.Now);
            }
        }


        //calculate Trailing Stop Loss
        protected double calcTrailingStopLoss(Position position)
        {
            double newStopLossPips = 0;
            double newStopLossPrice = 0;
            double currentStopLossPips = 0;
            double currentStopLossPrice = 0;

            bool isProtected = position.StopLoss.HasValue;
            if (isProtected)
            {
                currentStopLossPrice = (double)position.StopLoss;
            }
            else
            {
                //Should never happen
                Bot.Print("WARNING: Trailing Stop Loss Activated but No intial STOP LESS set");
                currentStopLossPrice = BotState.LastPositionEntryPrice;
            }

            if (position.TradeType == TradeType.Buy)
            {
                newStopLossPrice = Bot.Symbol.Ask - TrailingStopPips;
                newStopLossPips = position.EntryPrice - newStopLossPrice;
                currentStopLossPips = position.EntryPrice - currentStopLossPrice;

                //Is newStopLoss more risk than current SL
                if (newStopLossPips < currentStopLossPips)
                    return 0;

                //Is newStopLoss more than the current Ask and therefore not valid
                if (newStopLossPrice > Bot.Symbol.Ask)
                    return 0;

                //Is the difference between the newStopLoss and the current SL less than the tick size and therefore not valid
                if (currentStopLossPips - newStopLossPips < Bot.Symbol.TickSize)
                    return 0;
            }

            if (position.TradeType == TradeType.Sell)
            {
                newStopLossPrice = Bot.Symbol.Bid + TrailingStopPips;
                newStopLossPips = newStopLossPrice - position.EntryPrice;
                currentStopLossPips = currentStopLossPrice - position.EntryPrice;

                //Is newStopLoss more risk than current SL
                if (newStopLossPips > currentStopLossPips)
                    return 0;

                //Is newStopLoss more than the current Ask and therefore not valid
                if (newStopLossPrice < Bot.Symbol.Bid)
                    return 0;

                //Is the difference between the newStopLoss and the current SL less than the tick size and therefore not valid
                if (currentStopLossPips - newStopLossPips < Bot.Symbol.TickSize)
                    return 0;
            }

            return newStopLossPrice;
        }
    }
}
