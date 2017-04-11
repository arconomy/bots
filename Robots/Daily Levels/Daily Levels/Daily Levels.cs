using System;
using System.Linq;
using cAlgo.API;
using cAlgo.API.Indicators;
using cAlgo.API.Internals;
using cAlgo.Indicators;

namespace cAlgo
{
    [Robot(TimeZone = TimeZones.UTC, AccessRights = AccessRights.None)]
    public class DailyLevels : Robot
    {
        [Parameter(DefaultValue = 0.0)]
        public double Parameter { get; set; }



        [Parameter("TimeFrame")]
        public TimeFrame TF { get; set; }



        double CashHigh = int.MinValue;
        double CashLow = int.MaxValue;
        double CashOpen = int.MinValue;
        double CashClose = int.MinValue;


        protected override void OnStart()
        {
            // Put your initialization logic here

            var anotherSeries = MarketData.GetSeries(TF);


            Print("Opened: ");

            DateTime TodayUTC = MarketSeries.OpenTime.LastValue.ToUniversalTime().Subtract(MarketSeries.OpenTime.LastValue.ToUniversalTime().TimeOfDay);
            DateTime YesterdayUTC = TodayUTC.AddDays(-1);
            if (YesterdayUTC.DayOfWeek == DayOfWeek.Sunday)
                YesterdayUTC = TodayUTC.AddDays(-2);

            if (YesterdayUTC.DayOfWeek == DayOfWeek.Saturday)
                YesterdayUTC = TodayUTC.AddDays(-1);

            Print("YesterdayUTC: " + YesterdayUTC.ToString());

            int n = 0;
            while (true)
            {


                Print("in!");
                //Get the Day
                if (MarketSeries.OpenTime.Last(n).Date.ToUniversalTime() == YesterdayUTC.Date)
                {


                    Print("Got Day!");

                    if (MarketSeries.OpenTime.Last(n).ToUniversalTime().TimeOfDay >= new TimeSpan(8, 0, 0) & MarketSeries.OpenTime.Last(n).ToUniversalTime().TimeOfDay <= new TimeSpan(14, 30, 0))
                    {


                        Print("got time!");

                        if (CashHigh < MarketSeries.High.Last(n))
                            CashHigh = MarketSeries.High.Last(n);

                        if (CashLow > MarketSeries.Low.Last(n))
                            CashLow = MarketSeries.Low.Last(n);

                        if (MarketSeries.OpenTime.Last(n).ToUniversalTime().TimeOfDay == new TimeSpan(8, 0, 0))
                            CashOpen = MarketSeries.Open.Last(n);


                    }

                }

                if (MarketSeries.OpenTime.Last(n).ToUniversalTime() <= YesterdayUTC)
                    break;



                n += 1;
            }
            // while

            //Draw Lines:

            Print("out!");

            Print("drawn!" + CashHigh.ToString());


        }


        protected override void OnBar()
        {





            ChartObjects.DrawHorizontalLine("Previous Cash High", CashHigh, Colors.Blue, 2);
            ChartObjects.DrawHorizontalLine("Previous Cash Low", CashLow, Colors.BlueViolet, 2);



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
