using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using cAlgo.API;
using Niffler.Common.Market;
using Niffler.Common.BackTest;

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
        public bool IsAfterCloseTime { get; set; }
        public bool IsAfterReducedRiskTime { get; set; }
        public bool IsAfterTerminateTime { get; set; }

        public string BotId { get; set; }

        //Price and Position Variables
        public double OpenPrice { get; set; }
        public string LastPositionLabel { get; set; }
        public TradeType LastPositionTradeType { get; set; }
        public double LastPositionEntryPrice { get; set; }

        public double LastProfitPositionEntryPrice { get; set; }
        public double LastProfitPositionClosePrice { get; set; }

        public double OpenedPositionsCount = 0;
        public double ClosedPositionsCount = 0;
        public Robot Bot { get; set; }
        private MarketInfo MarketInfo;
        private ProfitReporter ProfitReporter;
        private SpikeManager SpikeManager;

        public State(Robot r)
        {
            reset();
            Bot = r;
            MarketInfo = new MarketInfo(Bot);
            ProfitReporter = new ProfitReporter(this);
            SpikeManager = new SpikeManager(this);
        }

        public string getMarketName()
        {
            return MarketInfo.MarketName;
        }

        public MarketInfo getMarketInfo()
        {
            return MarketInfo;
        }

        public void checkTimeState()
        {
            if (!IsAfterCloseTime && MarketInfo.IsAfterCloseTime())
                IsAfterCloseTime = true;

            if (!IsAfterReducedRiskTime && MarketInfo.IsAfterReduceRiskTime())
                IsAfterReducedRiskTime = true;

            if (!IsTerminated && !IsAfterTerminateTime && MarketInfo.IsAfterTerminateTime())
                IsAfterTerminateTime = true;
        }

        protected void reset()
        {
            if (IsReset)
                return;

            ProfitReporter.reportTotals(this);

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
            IsAfterCloseTime = false;
            IsAfterReducedRiskTime = false;
            IsAfterTerminateTime = false;
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
            IsActive = false;

            // swordfish bot state variables
            OpenPriceCaptured = false;
            OrdersPlaced = false;
            IsPendingOrdersClosed = false;
            IsTerminated = false;
            IsReset = true;
            IsReducedRiskTime = false;

            //Reset reporting and Spike variables
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
        public double calcPercentOfPositionsClosed()
        {
            double percentClosed = 0;
            if (OpenedPositionsCount > 0)
            {
                percentClosed = (ClosedPositionsCount / OpenedPositionsCount) * 100;
            }
            return percentClosed;
        }

        public void captureLastProfitPositionPrices(Position p)
        {
            LastProfitPositionClosePrice = Bot.History.FindLast(p.Label, Bot.Symbol, p.TradeType).ClosingPrice;
            LastProfitPositionEntryPrice = p.EntryPrice;
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

        protected string getReportSnapShot()
        {

            string state = "";
            // Position counters
            state += "," + OpenedPositionsCount;
            state += "," + ClosedPositionsCount;

            // Last Position variables
            state += "," + LastPositionEntryPrice;
            state += "," + LastClosedPositionEntryPrice;
            state += "," + LastProfitPrice;
            state += "," + LastPositionLabel;

            // risk management variables
            state += "," + IsBreakEvenStopLossActive;
            state += "," + IsHardSLLastClosedPositionEntryPrice;
            state += "," + IsHardSLLastPositionEntryPrice;
            state += "," + IsHardSLLastProfitPrice;

            // swordfish bot state variables
            state += "," + OpenPriceCaptured;
            state += "," + OrdersPlaced;
            state += "," + IsReducedRiskTime;
            state += "," + IsTerminated;
            state += "," + IsReset;

            return state;
        }

        public string getReportSnapShotHeaders()
        {
            return "OpenedPositionsCount" +
            ",ClosedPositionsCount" +
            ",LastPositionEntryPrice" +
            ",LastClosedPositionEntryPrice" +
            ",LastProfitPrice" +
            ",LastPositionLabel" +
            ",isBreakEvenStopLossActive" +
            ",isHardSLLastClosedPositionEntryPrice" +
            ",isHardSLLastPositionEntryPrice" +
            ",isHardSLLastProfitPrice" +
            ",OpenPriceCaptured" +
            ",OrdersPlaced" +
            ",isReducedRiskTime" +
            ",isTerminated" +
            ",isReset";
        }
           
         private List<String> getParameters()
        {

            List<string> parameters = new List<string>();

            Attribute[] parameterAttributes = ParameterAttribute.GetCustomAttributes(typeof(ParameterAttribute), true);

            foreach (Attribute attr in parameterAttributes)
            {
                parameters.Add(attr.ToString());
            }

            return parameters;

            /*
            parameters.Add("PARAMETERS");
            parameters.Add("useBollingerBandEntry," + Bot.useBollingerBandEntry.ToString());
            parameters.Add("targetBolliEntryPips," + targetBolliEntryPips.ToString());
            parameters.Add("SwordFishTrigger," + SwordFishTrigger.ToString());
            parameters.Add("OrderEntryOffset," + OrderEntryOffset.ToString());
            parameters.Add("OrderSpacing," + OrderSpacing.ToString());
            parameters.Add("OrderSpacingLevels," + OrderSpacingLevels.ToString());
            parameters.Add("OrderSpacingMultipler," + OrderSpacingMultipler.ToString());
            parameters.Add("OrderSpacingMax," + OrderSpacingMax.ToString());
            parameters.Add("NumberOfOrders," + NumberOfOrders.ToString());
            parameters.Add("Volume," + Volume.ToString());
            parameters.Add("VolumeMax," + VolumeMax.ToString());
            parameters.Add("OrderVolumeLevels," + OrderVolumeLevels.ToString());
            parameters.Add("VolumeMultipler," + VolumeMultipler.ToString());
            parameters.Add("TakeProfit," + TakeProfit.ToString());
            parameters.Add("ReducePositionRiskTime," + ReducePositionRiskTime.ToString());
            parameters.Add("retraceEnabled," + retraceEnabled.ToString());
            parameters.Add("retraceLevel1," + retraceLevel1.ToString());
            parameters.Add("retraceLevel2," + retraceLevel2.ToString());
            parameters.Add("retraceLevel3," + retraceLevel3.ToString());
            parameters.Add("FinalOrderStopLoss," + FinalOrderStopLoss.ToString());
            parameters.Add("HardStopLossBuffer," + HardStopLossBuffer.ToString());
            parameters.Add("TrailingStopPips," + TrailingStopPips.ToString());
            parameters.Add("--------------------------");
            */
         }
    }
}

