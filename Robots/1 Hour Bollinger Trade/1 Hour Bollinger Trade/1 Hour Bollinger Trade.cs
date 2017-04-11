using System;
using System.Linq;
using cAlgo.API;
using cAlgo.API.Indicators;
using cAlgo.API.Internals;
using cAlgo.Indicators;

namespace cAlgo
{
    [Robot(TimeZone = TimeZones.GMTStandardTime, AccessRights = AccessRights.None)]
    public class OneHourBollingerTrade : Robot
    {

        [Parameter("Source Series")]
        public DataSeries SourceSeries { get; set; }

        [Parameter("Bollinger Periods", DefaultValue = 20)]
        public int BollingerPeriods { get; set; }

        [Parameter("Bollinger Deviation", DefaultValue = 2)]
        public int BollingerDeviation { get; set; }

        [Parameter("Mollinger Moving Average Type", DefaultValue = MovingAverageType.Exponential)]
        public MovingAverageType BollingerMAType { get; set; }

        [Parameter("Contract Size", DefaultValue = 2.0)]
        public double ContractSize { get; set; }

        [Parameter("Bar Break Size", DefaultValue = 2)]
        public int BarBreakPointSize { get; set; }

        [Parameter("Signal Max Size", DefaultValue = 10)]
        public double SignalMaxBarSize { get; set; }

        [Parameter("StopLoss Max Size", DefaultValue = 10)]
        public double StopLossMaxBarSize { get; set; }

        [Parameter("Trade Validity in Mins.", DefaultValue = 9)]
        public int MaxMinuteValid { get; set; }

        protected override void OnStart()
        {
            // Put your initialization logic here


        }


        protected override void OnBar()
        {



            // Close open trades at EOD
            //   if (MarketSeries.OpenTime.Last(1).TimeOfDay >= new TimeSpan(4, 30, 0))
            // {
            //     var positions = Positions.FindAll("M3BolliTrade");
            //     foreach (Position position in positions)
            //     {
            //         ClosePosition(position);
            //     }

            // }


            if (IsBacktesting)
            {
                if (MarketSeries.OpenTime.Last(1).TimeOfDay >= new TimeSpan(8, 0, 0))
                {
                    if (MarketSeries.OpenTime.Last(1).TimeOfDay <= new TimeSpan(8, 45, 0))
                    {
                        Print("dont trade in dodgy data or you'll get a dodgy trade!");
                        return;
                    }
                }
            }




            if (IsBacktesting)
            {
                if (MarketSeries.OpenTime.Last(1).TimeOfDay >= new TimeSpan(19, 0, 0))
                {
                    Print("dont trade in dodgy data or you'll get a dodgy trade!");
                    return;

                }
            }





            BollingerBands Boli = Indicators.BollingerBands(SourceSeries, BollingerPeriods, BollingerDeviation, BollingerMAType);

            double BarLength = GetBarLength(1);
            if (BarLength >= SignalMaxBarSize)
            {
                Print("Bar too big");
                return;
            }


            // For Sell Orders:
            // Is Previous Previous Bar outside the Bolli 
            if (Boli.Top.Last(2) <= MarketSeries.Close.Last(2))
            {


                // Is Previous Bar Close within the Boli 
                if (Boli.Top.Last(1) >= MarketSeries.Close.Last(1))
                {


                    //Is Bar in reverse direction
                    if (GetBarDirection(1) == "DOWN")
                    {

                        //SELL

                        double Entry = (MarketSeries.Low.Last(1) - BarBreakPointSize);
                        double Stop = GetAboveTheBar(1, Entry);

                        if (StopLossMaxBarSize > Stop)
                            PlaceStopOrder(TradeType.Sell, Symbol, Symbol.QuantityToVolume(ContractSize), Entry, "M3BolliTrade", Stop, BarLength, MarketSeries.OpenTime.LastValue.AddMinutes(MaxMinuteValid));


                    }



                }

            }

            // For Buy Orders


            // Is Previous Previous Bar outside the Bolli 
            if (Boli.Bottom.Last(2) >= MarketSeries.Close.Last(2))
            {


                // Is Previous Bar Close within the Boli 
                if (Boli.Bottom.Last(1) <= MarketSeries.Close.Last(1))
                {


                    //Is Bar in reverse direction
                    if (GetBarDirection(1) == "UP")
                    {

                        double Entry = (MarketSeries.High.Last(1) + BarBreakPointSize);
                        double Stop = GetBelowTheBar(1, Entry);

                        if (StopLossMaxBarSize > Stop)
                            PlaceStopOrder(TradeType.Buy, Symbol, Symbol.QuantityToVolume(ContractSize), Entry, "M3BolliTrade", Stop, BarLength, MarketSeries.OpenTime.LastValue.AddMinutes(MaxMinuteValid));


                    }



                }








            }
        }




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


            return Math.Abs(MarketSeries.High.Last(BarIndex) - MarketSeries.Low.Last(BarIndex));


        }


        protected double GetAboveTheBar(int BarIndex, double Entry)
        {

            return (MarketSeries.High.Last(BarIndex) + BarBreakPointSize) - Entry;




        }

        protected double GetBelowTheBar(int BarIndex, double Entry)
        {

            return Entry - (MarketSeries.Low.Last(BarIndex) - BarBreakPointSize);



        }


        protected override void OnTick()
        {
            // Put your core logic here






        }

        protected override void OnStop()
        {
            // Put your deinitialization logic here
        }
    }
}
