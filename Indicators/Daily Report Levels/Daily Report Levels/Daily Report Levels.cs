using System;
using cAlgo.API;
using cAlgo.API.Internals;
using cAlgo.API.Indicators;
using cAlgo.Indicators;


namespace cAlgo
{
    [Indicator(IsOverlay = true, TimeZone = TimeZones.UTC, AccessRights = AccessRights.FullAccess)]
    public class DailyReportLevels : Indicator
    {

        [Parameter("Source")]
        public DataSeries Source { get; set; }


        [Parameter("Yesterdays High")]
        public double CashHigh { get; set; }

        [Parameter("Yesterdays Low")]
        public double CashLow { get; set; }

        [Parameter("Yesterdays Close")]
        public double CashClose { get; set; }

        [Parameter("Yesterdays Open")]
        public double CashOpen { get; set; }

        [Parameter("Daily ATR")]
        public double CashATR { get; set; }



        [Parameter("Weekly High")]
        public double WeeklyHigh { get; set; }

        [Parameter("Weekly Low")]
        public double WeeklyLow { get; set; }

        [Parameter("Weekly Close")]
        public double WeeklyClose { get; set; }


        [Parameter("Monthly High")]
        public double MonthlyHigh { get; set; }

        [Parameter("Monthly Low")]
        public double MonthlyLow { get; set; }

        [Parameter("Monthly Close")]
        public double MonthlyClose { get; set; }


        double P = 0;
        double R1 = 0;
        double R2 = 0;
        double R3 = 0;
        double S1 = 0;
        double S2 = 0;
        double S3 = 0;
        double CBOL = 0;
        double CBOS = 0;


        double WP = 0;
        double MP = 0;

        // Go to Manage References at the top and add System.Net from the .NET Framework... and change the Bots Access Rights to Full Access (at the top of the page!)
        protected void SlackMe(string Message, string Channel)
        {

            // if (Channel == "")
            // {
            //     Channel = "daily-levels";
            //  }
            //  System.Net.WebClient WC = new System.Net.WebClient();
            //   string Webhook = "https://hooks.slack.com/services/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";
            //   string Response = WC.UploadString(Webhook, "{\"text\": \"" + Message + "\", \"channel\": \"#" + Channel + "\"}");

        }

        protected override void Initialize()
        {
            // Initialize and create nested indicators

            var DailyStats = MarketData.GetSeries(TimeFrame.Minute30);


            DateTime TodayUTC = DailyStats.OpenTime.LastValue.ToUniversalTime().Subtract(DailyStats.OpenTime.LastValue.ToUniversalTime().TimeOfDay);
            DateTime YesterdayUTC = TodayUTC.AddDays(-1);
            if (YesterdayUTC.DayOfWeek == DayOfWeek.Sunday)
                YesterdayUTC = TodayUTC.AddDays(-2);

            if (YesterdayUTC.DayOfWeek == DayOfWeek.Saturday)
                YesterdayUTC = TodayUTC.AddDays(-1);

            Print("YesterdayUTC: " + YesterdayUTC.ToString());


            int n = 0;
            CashLow = int.MaxValue;

            while (true)
            {


                //Print("in!");
                //Get the Day
                if (DailyStats.OpenTime.Last(n).Date.ToUniversalTime() == YesterdayUTC.Date)
                {


                    //  Print("Got Day!");

                    if (DailyStats.OpenTime.Last(n).ToUniversalTime().TimeOfDay >= new TimeSpan(8, 0, 0) & DailyStats.OpenTime.Last(n).ToUniversalTime().TimeOfDay <= new TimeSpan(14, 30, 0))
                    {


                        //  Print("got time!");

                        if (CashHigh < DailyStats.High.Last(n))
                            CashHigh = DailyStats.High.Last(n);

                        if (CashLow > DailyStats.Low.Last(n))
                            CashLow = DailyStats.Low.Last(n);

                        if (DailyStats.OpenTime.Last(n).ToUniversalTime().TimeOfDay == new TimeSpan(8, 0, 0))
                            CashOpen = DailyStats.Open.Last(n);

                        if (DailyStats.OpenTime.Last(n).ToUniversalTime().TimeOfDay == new TimeSpan(14, 30, 0))
                            CashClose = DailyStats.Open.Last(n);

                    }

                }

                if (DailyStats.OpenTime.Last(n).ToUniversalTime() <= YesterdayUTC)
                    break;


                n += 1;
            }
            // while


            string Summary = Symbol.Code.ToString() + ": Yesterday's Open: " + CashOpen.ToString() + ", Close: " + CashClose.ToString() + ", High: " + CashHigh.ToString() + ", Low: " + CashLow.ToString();

            SlackMe(Summary, "daily-levels");
            ChartObjects.DrawText("Previous", Summary, StaticPosition.BottomRight, Colors.Red);


            var ATRSeries = MarketData.GetSeries(TimeFrame.Daily);
            AverageTrueRange ATR = Indicators.AverageTrueRange(ATRSeries, 5, MovingAverageType.Simple);
            ChartObjects.DrawText("ATR", "ATR: " + ATR.Result.LastValue.ToString() + ", 15% ATR: " + (ATR.Result.LastValue * 0.15).ToString() + ",30% ATR: " + (ATR.Result.LastValue * 0.3).ToString() + "", StaticPosition.TopRight, Colors.Red);


            ChartObjects.DrawText("ATR2", "ATR: " + CashATR.ToString() + ", 15% ATR: " + (CashATR * 0.15).ToString() + ",30% ATR: " + (CashATR * 0.3).ToString(), -20, Symbol.Ask, VerticalAlignment.Center, HorizontalAlignment.Center, Colors.Green);




            // DAILY 
            ChartObjects.DrawHorizontalLine("DailyHigh", CashHigh, Colors.Green, 1, LineStyle.LinesDots);
            ChartObjects.DrawHorizontalLine("DailyLow", CashLow, Colors.Green, 1, LineStyle.LinesDots);
            ChartObjects.DrawHorizontalLine("DailyCLose", CashClose, Colors.Green, 1, LineStyle.LinesDots);


            // Daily Levels


            P = ((CashHigh + CashLow + CashClose) / 3);


            R1 = ((2 * P) - CashLow);
            R2 = (P + CashHigh - CashLow);
            R3 = (CashHigh + 2 * (P - CashLow));


            S1 = ((2 * P) - CashHigh);
            S2 = (P - CashHigh + CashLow);
            S3 = CashLow - 2 * (CashHigh - P);

            CBOL = ((CashHigh - CashLow) * 1.1 / 2 + CashClose);
            CBOS = CashClose - (CashHigh - CashLow) * 1.1 / 2;



            WP = ((WeeklyHigh + WeeklyLow + WeeklyClose) / 3);
            MP = ((MonthlyHigh + MonthlyLow + MonthlyClose) / 3);


            ChartObjects.DrawHorizontalLine("DailyPivot", P, Colors.Black, 1, LineStyle.Solid);
            ChartObjects.DrawHorizontalLine("WeeklyPivot", WP, Colors.Black, 3, LineStyle.Lines);
            ChartObjects.DrawHorizontalLine("MonthlyPivot", MP, Colors.Black, 3, LineStyle.Solid);

            ChartObjects.DrawHorizontalLine("R1", R1, Colors.Red, 1, LineStyle.Solid);
            ChartObjects.DrawHorizontalLine("R2", R2, Colors.Red, 1, LineStyle.Solid);
            ChartObjects.DrawHorizontalLine("R3", R3, Colors.Red, 1, LineStyle.Solid);

            ChartObjects.DrawHorizontalLine("S1", S1, Colors.Green, 1, LineStyle.Solid);
            ChartObjects.DrawHorizontalLine("S2", S2, Colors.Green, 1, LineStyle.Solid);
            ChartObjects.DrawHorizontalLine("S3", S3, Colors.Green, 1, LineStyle.Solid);

            ChartObjects.DrawHorizontalLine("CBOL", CBOL, Colors.YellowGreen, 1, LineStyle.Solid);
            ChartObjects.DrawHorizontalLine("CBOS", CBOS, Colors.Blue, 1, LineStyle.Solid);


            // WEEKLY
            ChartObjects.DrawHorizontalLine("WeeklyHigh", WeeklyHigh, Colors.Green, 1, LineStyle.Lines);
            ChartObjects.DrawHorizontalLine("WeeklyLow", WeeklyLow, Colors.Red, 1, LineStyle.Lines);
            //   ChartObjects.DrawHorizontalLine("WeeklyClose", WeeklyClose, Colors.DeepSkyBlue, 1, LineStyle.LinesDots);


            // MONTHLY
            ChartObjects.DrawHorizontalLine("MonthlyHigh", MonthlyHigh, Colors.Green, 3, LineStyle.Lines);
            ChartObjects.DrawHorizontalLine("MonthlyLow", MonthlyLow, Colors.Red, 3, LineStyle.Lines);
            //   ChartObjects.DrawHorizontalLine("MonthlyClose", MonthlyClose, Colors.DarkGray, 1, LineStyle.LinesDots);




        }




        public override void Calculate(int index)
        {


            // DAILY
            ChartObjects.DrawText("DailyHigh", "Daily High", (index + 10), CashHigh, VerticalAlignment.Top, HorizontalAlignment.Left, Colors.Black);
            ChartObjects.DrawText("DailyLow", "Daily Low", (index + 10), CashLow, VerticalAlignment.Top, HorizontalAlignment.Left, Colors.Black);
            ChartObjects.DrawText("DailyClose", "Daily Close", (index + 10), CashClose, VerticalAlignment.Top, HorizontalAlignment.Left, Colors.Black);


            ChartObjects.DrawText("DailyPivot", "Daily Pivot", (index - 10), P, VerticalAlignment.Top, HorizontalAlignment.Left, Colors.Black);
            ChartObjects.DrawText("WeeklyPivot", "Weekly Pivot", (index - 10), WP, VerticalAlignment.Top, HorizontalAlignment.Left, Colors.Black);
            ChartObjects.DrawText("MonthlyPivot", "Monthly Pivot", (index - 10), MP, VerticalAlignment.Top, HorizontalAlignment.Left, Colors.Black);


            // Print(P);
            ChartObjects.DrawText("R1", "R1", (index + 4), R1, VerticalAlignment.Top, HorizontalAlignment.Left, Colors.Red);
            ChartObjects.DrawText("R2", "R2", (index + 4), R2, VerticalAlignment.Top, HorizontalAlignment.Left, Colors.Red);
            ChartObjects.DrawText("R3", "R3", (index + 4), R3, VerticalAlignment.Top, HorizontalAlignment.Left, Colors.Red);

            ChartObjects.DrawText("S1", "S1", (index + 6), S1, VerticalAlignment.Top, HorizontalAlignment.Left, Colors.Green);
            ChartObjects.DrawText("S2", "S2", (index + 6), S2, VerticalAlignment.Top, HorizontalAlignment.Left, Colors.Green);
            ChartObjects.DrawText("S3", "S3", (index + 6), S3, VerticalAlignment.Top, HorizontalAlignment.Left, Colors.Green);


            ChartObjects.DrawText("CBOL", "CBOL", (index + 8), CBOL, VerticalAlignment.Top, HorizontalAlignment.Left, Colors.YellowGreen);
            ChartObjects.DrawText("CBOS", "CBOS", (index + 8), CBOS, VerticalAlignment.Top, HorizontalAlignment.Left, Colors.Blue);



            // WEEKLY
            ChartObjects.DrawText("WeeklyHigh", "Weekly High", (index - 10), WeeklyHigh, VerticalAlignment.Top, HorizontalAlignment.Left, Colors.Green);
            ChartObjects.DrawText("WeeklyLow", "Weekly Low", (index - 20), WeeklyLow, VerticalAlignment.Top, HorizontalAlignment.Left, Colors.Black);
            //   ChartObjects.DrawText("WeeklyClose", "Weekly Close", (index - 30), WeeklyClose, VerticalAlignment.Top, HorizontalAlignment.Left, Colors.DeepSkyBlue);


            // MONTHLY
            ChartObjects.DrawText("MonthlyHigh", "Monthly High", (index - 40), MonthlyHigh, VerticalAlignment.Top, HorizontalAlignment.Left, Colors.Green);
            ChartObjects.DrawText("MonthlyLow", "Monthly Low", (index - 60), MonthlyLow, VerticalAlignment.Top, HorizontalAlignment.Left, Colors.Red);
            //   ChartObjects.DrawText("MonthlyClose", "Monthly Close", (index - 70), MonthlyClose, VerticalAlignment.Top, HorizontalAlignment.Left, Colors.DarkGray);



        }
    }
}
