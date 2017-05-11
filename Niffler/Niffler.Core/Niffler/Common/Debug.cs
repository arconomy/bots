using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Niffler.Common
{
    class Debug
    {

        //Performance Reporting
        protected double _dayProfitTotal = 0;
        protected double _dayPipsTotal = 0;

        List<string> debugCSV = new List<string>();


        public Debug()
        {
            writeColumnHeaders();
        }


        private void writeColumnHeaders()
        {
            debugCSV.Add("PARAMETERS");
            debugCSV.Add("useBollingerBandEntry," + useBollingerBandEntry.ToString());
            debugCSV.Add("targetBolliEntryPips," + targetBolliEntryPips.ToString());
            debugCSV.Add("SwordFishTrigger," + SwordFishTrigger.ToString());
            debugCSV.Add("OrderEntryOffset," + OrderEntryOffset.ToString());
            debugCSV.Add("OrderSpacing," + OrderSpacing.ToString());
            debugCSV.Add("OrderSpacingLevels," + OrderSpacingLevels.ToString());
            debugCSV.Add("OrderSpacingMultipler," + OrderSpacingMultipler.ToString());
            debugCSV.Add("OrderSpacingMax," + OrderSpacingMax.ToString());
            debugCSV.Add("NumberOfOrders," + NumberOfOrders.ToString());
            debugCSV.Add("Volume," + Volume.ToString());
            debugCSV.Add("VolumeMax," + VolumeMax.ToString());
            debugCSV.Add("OrderVolumeLevels," + OrderVolumeLevels.ToString());
            debugCSV.Add("VolumeMultipler," + VolumeMultipler.ToString());
            debugCSV.Add("TakeProfit," + TakeProfit.ToString());
            debugCSV.Add("ReducePositionRiskTime," + ReducePositionRiskTime.ToString());
            debugCSV.Add("retraceEnabled," + retraceEnabled.ToString());
            debugCSV.Add("retraceLevel1," + retraceLevel1.ToString());
            debugCSV.Add("retraceLevel2," + retraceLevel2.ToString());
            debugCSV.Add("retraceLevel3," + retraceLevel3.ToString());
            debugCSV.Add("FinalOrderStopLoss," + FinalOrderStopLoss.ToString());
            debugCSV.Add("HardStopLossBuffer," + HardStopLossBuffer.ToString());
            debugCSV.Add("TrailingStopPips," + TrailingStopPips.ToString());
            debugCSV.Add("--------------------------");

            debugCSV.Add("Trade,Profit,Pips,Day,Label,EntryPrice,ClosePrice,SL,TP,Date/Time,OpenedPositionsCount,ClosedPositionsCount,LastPositionEntryPrice,LastClosedPositionEntryPrice,LastProfitPrice,LastPositionLabel,DivideTrailingStopPips,isTrailingStopsActive,isBreakEvenStopLossActive,isHardSLLastClosedPositionEntryPrice,isHardSLLastPositionEntryPrice,isHardSLLastProfitPrice,OpenPriceCaptured,OrdersPlaced,isSwordFishReset,isSwordfishTerminated,isReducedRiskTime");
        }

        public void writeTrade()
        {
            debugCSV.Add("TRADE," + args.Position.GrossProfit + "," + args.Position.Pips + "," + Time.DayOfWeek + "," + args.Position.Label + "," + args.Position.EntryPrice + "," + History.FindLast(args.Position.Label, Symbol, args.Position.TradeType).ClosingPrice + "," + args.Position.StopLoss + "," + args.Position.TakeProfit + "," + Time + debugState());
        }


        protected string writeState()
        {

            string state = "";
            // Position counters
            state += "," + Bot_openedPositionsCount;
            state += "," + _closedPositionsCount;

            // Last Position variables
            state += "," + BotState.LastPositionEntryPrice;
            state += "," + BotState.LastClosedPositionEntryPrice;
            state += "," + BotState.LastProfitPrice;
            state += "," + BotState.LastPositionLabel;

            // risk management variables
            state += "," + FixedTrailingStop.TrailingStopPips;
            state += "," + FixedTrailingStop.IsTrailingStopsActive;
            state += "," + BotState.IsBreakEvenStopLossActive;
            state += "," + BotState.IsHardSLLastClosedPositionEntryPrice;
            state += "," + BotState.IsHardSLLastPositionEntryPrice;
            state += "," + BotState.IsHardSLLastProfitPrice;

            // swordfish bot state variables
            state += "," + BotState.IsHardSLLastProfitPrice;
            state += "," + BotState.OrdersPlaced;
            state += "," + BotState.IsReset;
            state += "," + BotState.IsTerminated;
            state += "," + BotState.IsReducedRiskTime;

            return state;
        }


        public void reportDay(Spike spike)
        {

            string profit = "";
            if (_dayProfitTotal != 0 && _dayPipsTotal != 0)
            {
                profit = ("DAY TOTAL," + _dayProfitTotal + "," + _dayPipsTotal + "," + _openedPositionsCount + "," + _spikePeakPips + "," + Time.DayOfWeek + "," + Time);
                debugCSV.Add(profit);
            }


            // reset reporting variables
            _dayProfitTotal = 0;
            _dayPipsTotal = 0;
            _spikePeakPips = 0;
            _spikePeakPrice = 0;
        }

        public void WriteToFile()
        {
            System.IO.File.WriteAllLines("C:\\Users\\alist\\Desktop\\swordfish\\" + _swordFishTimeInfo.market + "-" + _botId + "-" + "swordfish-" + getTimeStamp(true) + ".csv", debugCSV.ToArray());
        }



    }
}
