using System;
using System.Collections.Generic;
using cAlgo.API;
using Niffler.Common.Market;
using Niffler.Common.BackTest;
using Niffler.Microservices;
using Niffler.Messaging.RabbitMQ;

namespace Niffler.Common
{
    class State
    { 

        public string BotId { get; set; }

        //Only State variable that is not Reset - it is set in a rule to start trading and a rule to stop trading
        public bool IsTrading { get; set; }

        //State Variables
        public bool IsPendingOrdersClosed { get; set; }
        public bool OpenPriceCaptured { get; set; }
        public bool OrdersPlaced { get; set; }
        public bool IsReset { get; set; }
        public bool IsOpenTime { get; set; }
        public bool IsAfterCloseTime { get; set; }
        public bool IsAfterReducedRiskTime { get; set; }
        public bool IsAfterTerminateTime { get; set; }

        //Price and Position Variables
        public double OpenPrice { get; set; }
        public string LastPositionLabel { get; set; }
        public TradeType LastPositionTradeType { get; set; }

        public double LastPositionEntryPrice { get; set; }
        public double LastProfitPositionEntryPrice { get; set; }
        public double LastProfitPositionClosePrice { get; set; }
        public double OpenedPositionsCount { get; set; }
        public double ClosedPositionsCount { get; set; }

        private bool IsBackTesting;
        private bool IsTradeMonday = true;
        private bool IsTradeTuesday = true;
        private bool IsTradeWednesday = true;
        private bool IsTradeThursday = true;
        private bool IsTradeFriday = true;
        private bool IsTradeSaturday = false;
        private bool IsTradeSunday = false;

        public String MarketName { get; set; }

        public string BotName { get; set; }
        private TimeInfo MarketInfo;
        private Reporter Reporter;
        private SpikeManager SpikeManager;

        public State(string botId)
        {
            BotId = botId;
        }

        public string GetMarketName()
        {
            return MarketInfo.MarketName;
        }

        public TimeInfo GetMarketInfo()
        {
            return MarketInfo;
        }

        public Reporter GetReporter()
        {
            return Reporter;
        }
        
        public bool PositionsRemainOpen()
        {
            return OpenedPositionsCount > 0 && OpenedPositionsCount - ClosedPositionsCount > 0;
        }

        public bool PositionsAllClosed()
        {
            return OpenedPositionsCount > 0 && OpenedPositionsCount - ClosedPositionsCount == 0;
        }

        public bool PositionsNotOpened()
        {
            return OpenedPositionsCount == 0;
        }


        //Calculate the % of positions closed
        public double CalcPercentOfPositionsClosed()
        {
            double percentClosed = 0;
            if (OpenedPositionsCount > 0)
            {
                percentClosed = (ClosedPositionsCount / OpenedPositionsCount) * 100;
            }
            return percentClosed;
        }

        //Check whether a position or order is managed by this bot instance.
        public bool IsThisBotId(string label)
        {
            string id = label.Substring(0, 5);
            if (id.Equals(BotId))
                return true;
            else
                return false;
        }

        public string GetReportSnapShot()
        {

            string state = "";
            // Position counters
            state += "," + OpenedPositionsCount;
            state += "," + ClosedPositionsCount;

            // Last Position variables
            state += "," + LastPositionEntryPrice;
            state += "," + LastProfitPositionEntryPrice;
            state += "," + LastProfitPositionClosePrice;
            state += "," + LastPositionLabel;

            // swordfish bot state variables
            state += "," + OpenPriceCaptured;
            state += "," + OrdersPlaced;
            state += "," + IsAfterCloseTime;
            state += "," + IsAfterReducedRiskTime;
            state += "," + IsOpenTime;

            return state;
        }

        public string GetReportSnapShotHeaders()
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
           
         private List<String> GetParameters()
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

