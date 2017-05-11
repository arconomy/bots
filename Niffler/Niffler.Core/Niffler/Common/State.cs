using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using cAlgo.API;

namespace Niffler.Common
{
    class State
    {
        public int id { get; set; }

        //Stop Loss Variables
        public bool IsHardSLLastProfitPrice { get; set; }
        public bool IsHardSLLastPositionEntryPrice { get; set; }
        public bool IsHardSLLastClosedPositionEntryPrice { get; set; }
        public bool IsBreakEvenStopLossActive { get; set; }

        //Swordfish State Variables
        public bool IsPendingOrdersClosed { get; set; }
        public bool OpenPriceCaptured { get; set; }
        public bool OrdersPlaced { get; set; }
        public bool IsTerminated { get; set; }
        public bool IsReset { get; set; }
        public bool IsReducedRiskTime { get; set; }
        public string BotId { get; set; }

        //Price and Position Variables
        public double OpenPrice { get; set; }
        public string LastPositionLabel { get; set; }
        public TradeType LastPositionTradeType { get; set; }
        public double LastPositionEntryPrice { get; set; }
        public double LastClosedPositionEntryPrice { get; set; }
        public double LastProfitPrice { get; set; }

        public double OpenedPositionsCount = 0;
        public double ClosedPositionsCount = 0;


        public State()
        {
            reset();
        }

        protected void reset()
        {
            if (IsReset)
                return;

            Debug.reportDay();

            //Set default Stop Loss Variables
            IsHardSLLastProfitPrice = false;
            IsHardSLLastPositionEntryPrice = false;
            IsHardSLLastClosedPositionEntryPrice = false;
            IsBreakEvenStopLossActive = false;

            //Set default Swordfish State Variables
            IsPendingOrdersClosed = false;
            OpenPriceCaptured = false;
            OrdersPlaced = false;
            IsTerminated = false;
            IsReset = true;
            IsReducedRiskTime = false;
            BotId = generateBotId();

            //Price and Position Variables
            OpenedPositionsCount = 0;
            ClosedPositionsCount = 0;

            //reset Last Position variables
            LastPositionLabel = "NO LAST POSITION SET";
            LastPositionEntryPrice = 0;
            LastClosedPositionEntryPrice = 0;
            LastProfitPrice = 0;

            //reset risk management variables
            IsBreakEvenStopLossActive = false;
            IsHardSLLastClosedPositionEntryPrice = false;
            IsHardSLLastPositionEntryPrice = false;
            IsHardSLLastProfitPrice = false;

            //ResetTrailingStops
            _divideTrailingStopPips = 1;
            IsTrailingStopsActive = false;

            // swordfish bot state variables
            OpenPriceCaptured = false;
            OrdersPlaced = false;
            IsPendingOrdersClosed = false;
            IsTerminated = false;
            IsReset = true;
            IsReducedRiskTime = false;
        }


        private string generateBotId()
        {
            Random randomIdGenerator = new Random();
            int id = randomIdGenerator.Next(0, 99999);
            return id.ToString("00000");
        }


        public bool positionsRemainOpen()
        {
            return OpenedPositionsCount - ClosedPositionsCount> 0;
        }

        public bool allPositionsClosed()
        {
            return OpenedPositionsCount > 0 && OpenedPositionsCount - ClosedPositionsCount == 0;
        }


        //Calculate the % of positions closed
        protected double calculatePercentageClosed()
        {
            double percentClosed = 0;
            if (_openedPositionsCount > 0)
            {
                percentClosed = (_closedPositionsCount / _openedPositionsCount) * 100;
            }
            return percentClosed;
        }

        protected void setLastProfitPrice(TradeType lastProfitTradeType)
        {
            if (lastProfitTradeType == TradeType.Buy)
                BotState.LastProfitPrice = Symbol.Ask;
            if (lastProfitTradeType == TradeType.Sell)
                BotState.LastProfitPrice = Symbol.Bid;
        }


        //Check whether a position or order is managed by this bot instance.
        public bool isThisBotId(string label)
        {
            string id = label.Substring(0, 5);
            if (id.Equals(BotId))
                return true;
            else
                return false;
        }

    }
}

