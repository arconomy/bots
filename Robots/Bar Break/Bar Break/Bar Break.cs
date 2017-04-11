using System;
using System.Collections;
using System.Linq;
using cAlgo.API;
using cAlgo.API.Indicators;
using cAlgo.API.Internals;
using cAlgo.Indicators;

namespace cAlgo
{
    [Robot(TimeZone = TimeZones.GMTStandardTime)]
    public class BarBreak : Robot
    {

        [Parameter("Enable INSANE Mode?", DefaultValue = 1)]
        public bool InsaneModeEnabled { get; set; }


        [Parameter("Manage Risk Enabled", DefaultValue = true)]
        public bool UseAccountPercent { get; set; }

        [Parameter("-- (yes) Account Percentage (%)", DefaultValue = 1)]
        public double AccountPercent { get; set; }

        [Parameter("-- (no) Static Contract Size", DefaultValue = 1)]
        public int ContractSize { get; set; }


        [Parameter("Trading Enabled", DefaultValue = true)]
        public bool TradingEnabled { get; set; }

        [Parameter("-- Valid Days", DefaultValue = "MON,TUE,WED")]
        public string ValidDays { get; set; }

        [Parameter("-- Signal Bar Hour", DefaultValue = 7)]
        public int SignalBarHour { get; set; }

        [Parameter("-- Signal Bar Minute", DefaultValue = 30)]
        public int SignalBarMinute { get; set; }

        [Parameter("-- Bar Break Point Size", DefaultValue = 2)]
        public int BarBreakPointSize { get; set; }

        [Parameter("-- Valid After Bar Break (mins)", DefaultValue = 60)]
        public int MaxMinuteValid { get; set; }

        [Parameter("-- Min 5 Day ATR", DefaultValue = 80)]
        public int MinATR { get; set; }


        [Parameter("Reversal Trade Enabled", DefaultValue = true)]
        public bool ReversalTradeEnabled { get; set; }

        [Parameter("-- Contract Multiplier", DefaultValue = 2)]
        public int ReversalContractMultiplier { get; set; }

        [Parameter("-- Valid after Bar Break SL (mins)", DefaultValue = 13)]
        public int ReversalValidMinutes { get; set; }

        [Parameter("-- Point Break Size", DefaultValue = 2)]
        public int ReversalBarBreakPointSize { get; set; }


        [Parameter("Protect Position Enabled", DefaultValue = false)]
        public bool ProtectPosition { get; set; }


        [Parameter("-- Triggered Level (%)", DefaultValue = 75)]
        public double TriggerPercent { get; set; }

        [Parameter("-- Close Level (%)", DefaultValue = 25)]
        public double ClosePercent { get; set; }


        [Parameter("-- Activate after (hours)", DefaultValue = 3)]
        public double PositionActivateHours { get; set; }




        protected override void OnStart()
        {

            Positions.Opened += PositionsOnOpened;
            Positions.Closed += PositionsOnClosed;

        }

        protected long GetContractSize()
        {
            // Static Contact Size
            if (!UseAccountPercent)
                return ContractSize;

            //Calculate Contract Sie based off account balanace...
            return 1;


        }



        protected override void OnBar()
        {

            if (!TradingEnabled)
                return;

            // Check the Day of Week for Trading.... 
            bool OkToTradeToday = false;
            foreach (string DOW in ValidDays.Split(','))
            {
                if (MarketSeries.OpenTime.LastValue.DayOfWeek.ToString().ToLower().Contains(DOW.ToLower()))
                    OkToTradeToday = true;
            }

            if (!OkToTradeToday)
                return;




            // Check that the last Bar was the Open.
            if (MarketSeries.OpenTime.Last(1).TimeOfDay == new TimeSpan(SignalBarHour, SignalBarMinute, 0))
            {

                double BarLength = GetBarLength(1);

                MarketSeries MarketSeriesDaily = MarketData.GetSeries(TimeFrame.Daily);
                AverageTrueRange ATR = Indicators.AverageTrueRange(MarketSeriesDaily, 5, MovingAverageType.Simple);

                if (ATR.Result.LastValue >= MinATR)
                {


                    if (GetBarDirection(1) != "NEUTRAL")
                    {

                        TradeType Trade;
                        double Entry;
                        double SL;
                        double TP = BarLength * (1 / Symbol.TickSize);
                        long Contracts = GetContractSize();

                        if (GetBarDirection(1) == "DOWN")
                        {
                            Trade = TradeType.Sell;
                            Entry = MarketSeries.Low.Last(1) - BarBreakPointSize;
                            SL = (MarketSeries.High.Last(1) + BarBreakPointSize) - Entry;

                            //Increasing the SL a little more 
                            SL = SL + 8;

                        }
                        else
                        {
                            Trade = TradeType.Buy;
                            Entry = MarketSeries.High.Last(1) + BarBreakPointSize;
                            SL = Entry - (MarketSeries.Low.Last(1) - BarBreakPointSize);

                            //Increasing the SL a little more 
                            SL = SL + 8;
                        }

                        SL = SL * (1 / Symbol.TickSize);

                        var Result = PlaceStopOrder(Trade, Symbol, Contracts, Entry, "BarBreak", SL, TP, MarketSeries.OpenTime.LastValue.AddMinutes(MaxMinuteValid));

                        if (Result.IsSuccessful)
                        {
                            string OrderPlacedSummary = "Order Placed! BarLength: " + BarLength.ToString() + ", " + Trade.ToString() + ": " + Symbol.Code.ToString() + ", Entry: " + Entry.ToString() + ", Stop: " + SL.ToString() + ", TakeProfit: " + TP.ToString();
                            Print(OrderPlacedSummary);
                            Notifications.SendEmail("trading@afb.one", "andrewfblake@gmail.com", "New Position Opened", OrderPlacedSummary);
                        }
                        else
                        {
                            Print("Error: " + Result.Error);
                        }







                    }


                }
                else
                {
                    Print("5 Day ATR (" + ATR.Result.LastValue.ToString() + ") was outside acceptable boundaries.");
                }



            }





        }


        protected void PositionsOnClosed(PositionClosedEventArgs args)
        {

            // Reset Global Variables:
            ReachedMinTradePercent = false;

            //Validation... 

            if (!ReversalTradeEnabled)
            {
                Print("Reversal Trade not valid on this day.");
                return;
            }


            if (args.Position.Label == "BarBreakReversal" || args.Position.GrossProfit >= 0)
            {
                Print("Reversal trade not valid.");
                return;
            }


            // Check the Day of Week for Trading.... 
            bool OkToTradeToday = false;
            foreach (string DOW in ValidDays.Split(','))
            {
                if (MarketSeries.OpenTime.LastValue.DayOfWeek.ToString().ToLower().Contains(DOW.ToLower()))
                    OkToTradeToday = true;
            }

            if (!OkToTradeToday)
                return;






            // Create Market Oder:
            double BarLength = Math.Abs((double)args.Position.EntryPrice - (double)args.Position.StopLoss);
            TradeType Trade;
            double Entry;
            double SL = BarLength * (1 / Symbol.TickSize);
            double TP = BarLength * (1 / Symbol.TickSize);

            SL = SL - ReversalBarBreakPointSize;
            TP = TP - ReversalBarBreakPointSize;

            long Contracts = (GetContractSize() * ReversalContractMultiplier);

            if (args.Position.TradeType == TradeType.Buy)
            {
                //Sell at double stakes...
                Trade = TradeType.Sell;
                Entry = (double)args.Position.StopLoss - ReversalBarBreakPointSize;

            }
            else
            {
                //Buy at double stakes.. 
                Trade = TradeType.Buy;
                Entry = (double)args.Position.StopLoss + ReversalBarBreakPointSize;

            }


            var Result = PlaceStopOrder(Trade, Symbol, Contracts, Entry, "BarBreakReversal", SL, TP, MarketSeries.OpenTime.LastValue.AddMinutes(ReversalValidMinutes));

            if (Result.IsSuccessful)
            {
                string OrderPlacedSummary = "Order Placed! BarLength: " + BarLength.ToString() + ", " + Trade.ToString() + ": " + Symbol.Code.ToString() + ", Entry: " + Entry.ToString() + ", Stop: " + SL.ToString() + ", TakeProfit: " + TP.ToString();
                Print(OrderPlacedSummary);
                Notifications.SendEmail("trading@afb.one", "andrewfblake@gmail.com", "New Position Opened", OrderPlacedSummary);
            }
            else
            {
                Print("Error: " + Result.Error);
            }




        }

        protected void PositionsOnOpened(PositionOpenedEventArgs args)
        {


            // INSANE MODE! ... We know we're right, so while it goes the other way.. lets increase our average posistion

            if (InsaneModeEnabled && args.Position.Label == "BarBreak")
            {

                double Entry = 0.0;
                double SL = 0.0;
                double TP = 0.0;

                Print("Insane SL: " + SL.ToString());
                Print("Insane TP: " + TP.ToString());

                TradeType Trade = args.Position.TradeType;


                if (args.Position.TradeType == TradeType.Buy)
                {
                    Entry = (double)args.Position.StopLoss + 5;
                    SL = 10;
                    TP = 10;
                }
                else
                {

                    Entry = (double)args.Position.StopLoss - 5;
                    SL = 10;
                    TP = 10;

                }


                var Result = PlaceStopOrder(args.Position.TradeType, Symbol, ContractSize, Entry, "InsaneBarBreak", SL, TP, MarketSeries.OpenTime.LastValue.AddMinutes(MaxMinuteValid));


            }
            // Insane mode and position broken



        }


        protected override void OnTick()
        {





            foreach (Position Position in Positions)
            {
                ProtectProfit(Position, TriggerPercent, ClosePercent);
            }


        }

        protected override void OnStop()
        {


        }




        // 
        //  Helper Functions.
        // 

        protected string GetBarDirection(int BarIndex)
        {


            if (MarketSeries.Close.Last(BarIndex) < MarketSeries.Open.Last(BarIndex))
                return "DOWN";

            if (MarketSeries.Close.Last(BarIndex) > MarketSeries.Open.Last(BarIndex))
                return "UP";


            return "NEUTRAL";

        }

        protected double GetBarLength(int BarIndex)
        {


            double Length = Math.Abs(MarketSeries.High.Last(BarIndex) - MarketSeries.Low.Last(BarIndex));
            if (Length < 1)
                Length = 1;

            return Length;

        }

        protected double GetAboveTheBar(int BarIndex, double Entry)
        {

            return (MarketSeries.High.Last(BarIndex)) - Entry;

        }

        protected double GetBelowTheBar(int BarIndex, double Entry)
        {

            return Entry - (MarketSeries.Low.Last(BarIndex));

        }



        protected bool ReachedMinTradePercent = false;
        protected void ProtectProfit(Position Position, double TriggerPercentOfTrade, double ProtectedPercentOfTrade)
        {

            // Not yet in Profit, skip.... 
            if (Position.GrossProfit <= 0)
                return;


            // calculate percentage filled
            double PercentageFilled = 0.0;
            double FillLength = Math.Abs((double)Position.EntryPrice - (double)Position.TakeProfit);

            if (Position.TradeType == TradeType.Buy)
                PercentageFilled = ((Symbol.Ask - Position.EntryPrice) / FillLength) * 100;

            if (Position.TradeType == TradeType.Sell)
                PercentageFilled = ((Position.EntryPrice - Symbol.Ask) / FillLength) * 100;


            // if greater than x% then set Min target reach...
            if (PercentageFilled >= TriggerPercentOfTrade)
            {
                ReachedMinTradePercent = true;
                Print("Trade hit Protected percent... Position protected.");
            }



            //if min target reached... make sure it closes before min loss...
            if (ReachedMinTradePercent && Position.EntryTime <= MarketSeries.OpenTime.LastValue.AddHours(-PositionActivateHours))
            {

                if (PercentageFilled <= ProtectedPercentOfTrade)
                {
                    ClosePosition(Position);
                    Print("Closed Position as it hit the minimum amount");
                }

            }





        }




    }
}
