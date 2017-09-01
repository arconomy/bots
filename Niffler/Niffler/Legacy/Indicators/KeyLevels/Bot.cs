using cAlgo.API;
using cAlgo.API.Indicators;
using cAlgo.API.Internals;


namespace Niffler.Indicators.KeyLevels
{

    public class Bot : cAlgo.API.Indicator
    {


        [Parameter("Source")]
        public virtual DataSeries Source { get; set; }


         
        public virtual IndicatorDataSeries DailyOpenSeries { get; set; }
         
        public virtual IndicatorDataSeries DailyCloseSeries { get; set; }
         
        public virtual IndicatorDataSeries DailyHighSeries { get; set; }
         
        public virtual IndicatorDataSeries DailyLowSeries { get; set; }

         
        public virtual IndicatorDataSeries DailyR1Series { get; set; }
         
        public virtual IndicatorDataSeries DailyR2Series { get; set; }

        public virtual IndicatorDataSeries DailyR3Series { get; set; }


        public virtual IndicatorDataSeries DailyS1Series { get; set; }

        public virtual IndicatorDataSeries DailyS2Series { get; set; }

        public virtual IndicatorDataSeries DailyS3Series { get; set; }


        public virtual IndicatorDataSeries DailyCBOLSeries { get; set; }

        public virtual IndicatorDataSeries DailyCBOSSeries { get; set; }

        public virtual IndicatorDataSeries DailyPivotSeries { get; set; }



        //[Parameter("Weekly High")]
        //public virtual double WeeklyHigh { get; set; }

        //[Parameter("Weekly Low")]
        //public virtual double WeeklyLow { get; set; }

        //[Parameter("Weekly Close")]
        //public virtual double WeeklyClose { get; set; }


        //[Parameter("Monthly High")]
        //public virtual double MonthlyHigh { get; set; }

        //[Parameter("Monthly Low")]
        //public virtual double MonthlyLow { get; set; }

        //[Parameter("Monthly Close")]
        //public virtual double MonthlyClose { get; set; }


        double P = 0;
        double R1 = 0;
        double R2 = 0;
        double R3 = 0;
        double S1 = 0;
        double S2 = 0;
        double S3 = 0;
        double CBOL = 0;
        double CBOS = 0;


      //  double WP = 0;
       // double MP = 0;

        Model.KeyLevel YesterdayKeyLevels = null;
         
        protected override void Initialize()
        {
 

            // Summary
            YesterdayKeyLevels = Business.KeyLevels.GetYesterdaysKeyLevels(Account.BrokerName, Symbol.Code);

            if (YesterdayKeyLevels != null)
            {

                YesterdayKeyLevels.CalculateDaily();

                // Write Summary
                string Summary = Symbol.Code.ToString() + ": Yesterday's (" + YesterdayKeyLevels.Date.ToShortDateString() + ") Open: " + YesterdayKeyLevels.Open.ToString() + ", Close: " + YesterdayKeyLevels.Close.ToString() + ", High: " + YesterdayKeyLevels.High.ToString() + ", Low: " + YesterdayKeyLevels.Low.ToString();
                ChartObjects.DrawText("Previous", Summary, StaticPosition.BottomRight, Colors.Red);

                // Calculate ATR
                var ATRSeries = MarketData.GetSeries(TimeFrame.Daily);
                AverageTrueRange ATR = Indicators.AverageTrueRange(ATRSeries, 5, MovingAverageType.Simple);
                ChartObjects.DrawText("ATR", "ATR: " + ATR.Result.LastValue.ToString("0.##") + ", 15% ATR: " + (ATR.Result.LastValue * 0.15).ToString("0.##") + ",30% ATR: " + (ATR.Result.LastValue * 0.3).ToString("0.##") + "", StaticPosition.TopRight, Colors.Red);
                 
                // DAILY 
              //  ChartObjects.DrawHorizontalLine("DailyHigh", YesterdayKeyLevels.High,  Colors.Green, 1, LineStyle.LinesDots);
              //  ChartObjects.DrawHorizontalLine("DailyLow", YesterdayKeyLevels.Low, Colors.Green, 1, LineStyle.LinesDots);
               // ChartObjects.DrawHorizontalLine("DailyCLose", YesterdayKeyLevels.Close, Colors.Green, 1, LineStyle.LinesDots);

            

                // Daily Levels

                P = ((YesterdayKeyLevels.High + YesterdayKeyLevels.Low + YesterdayKeyLevels.Close) / 3);
                 
                R1 = ((2 * P) - YesterdayKeyLevels.Low);
                R2 = (P + YesterdayKeyLevels.High - YesterdayKeyLevels.Low);
                R3 = (YesterdayKeyLevels.High + 2 * (P - YesterdayKeyLevels.Low));
                 
                S1 = ((2 * P) - YesterdayKeyLevels.High);
                S2 = (P - YesterdayKeyLevels.High + YesterdayKeyLevels.Low);
                S3 = YesterdayKeyLevels.Low - 2 * (YesterdayKeyLevels.High - P);

                CBOL = ((YesterdayKeyLevels.High - YesterdayKeyLevels.Low) * 1.1 / 2 + YesterdayKeyLevels.Close);
                CBOS = YesterdayKeyLevels.Close - (YesterdayKeyLevels.High - YesterdayKeyLevels.Low) * 1.1 / 2;


              
                //WP = ((WeeklyHigh + WeeklyLow + WeeklyClose) / 3);
                //MP = ((MonthlyHigh + MonthlyLow + MonthlyClose) / 3);




                // WEEKLY
                // ChartObjects.DrawHorizontalLine("WeeklyHigh", WeeklyHigh, Colors.Green, 1, LineStyle.Lines);
                //ChartObjects.DrawHorizontalLine("WeeklyLow", WeeklyLow, Colors.Red, 1, LineStyle.Lines);
                //   ChartObjects.DrawHorizontalLine("WeeklyClose", WeeklyClose, Colors.DeepSkyBlue, 1, LineStyle.LinesDots);


                // MONTHLY
                //ChartObjects.DrawHorizontalLine("MonthlyHigh", MonthlyHigh, Colors.Green, 3, LineStyle.Lines);
                //ChartObjects.DrawHorizontalLine("MonthlyLow", MonthlyLow, Colors.Red, 3, LineStyle.Lines);
                //   ChartObjects.DrawHorizontalLine("MonthlyClose", MonthlyClose, Colors.DarkGray, 1, LineStyle.LinesDots);

            }


        }




        public override void Calculate(int index)
        {


            if (YesterdayKeyLevels != null)
            {

                DailyOpenSeries[index] = YesterdayKeyLevels.Open;
                DailyCloseSeries[index] = YesterdayKeyLevels.Close;
                DailyHighSeries[index] = YesterdayKeyLevels.High;
                DailyLowSeries[index] = YesterdayKeyLevels.Low;

                DailyR1Series[index] = R1;
                DailyR2Series[index] = R2;
                DailyR3Series[index] = R3;
               
                DailyS1Series[index] = S1;
                DailyS2Series[index] = S2;
                DailyS3Series[index] = S3;

                DailyCBOLSeries[index] = CBOL;
                DailyCBOSSeries[index] = CBOS;

                DailyPivotSeries[index] = P;

                //  ChartObjects.DrawHorizontalLine("DailyPivot", P, Colors.Gray, 1, LineStyle.Solid);
                // ChartObjects.DrawHorizontalLine("WeeklyPivot", WP, Colors.Black, 3, LineStyle.Lines);
                // ChartObjects.DrawHorizontalLine("MonthlyPivot", MP, Colors.Black, 3, LineStyle.Solid);

                // ChartObjects.DrawHorizontalLine("R1", R1, Colors.Red, 1, LineStyle.Solid);
                // ChartObjects.DrawHorizontalLine("R2", R2, Colors.Red, 1, LineStyle.Solid);
                //ChartObjects.DrawHorizontalLine("R3", R3, Colors.Red, 1, LineStyle.Solid);

                //ChartObjects.DrawHorizontalLine("S1", S1, Colors.Green, 1, LineStyle.Solid);
                //ChartObjects.DrawHorizontalLine("S2", S2, Colors.Green, 1, LineStyle.Solid);
                //ChartObjects.DrawHorizontalLine("S3", S3, Colors.Green, 1, LineStyle.Solid);

                //ChartObjects.DrawHorizontalLine("CBOL", CBOL, Colors.YellowGreen, 1, LineStyle.Solid);
                //ChartObjects.DrawHorizontalLine("CBOS", CBOS, Colors.Blue, 1, LineStyle.Solid);
                //// DAILY


                ChartObjects.DrawText("DailyHigh", "Daily High", (index + 10), YesterdayKeyLevels.High, VerticalAlignment.Top, HorizontalAlignment.Left, Colors.Black);
                ChartObjects.DrawText("DailyLow", "Daily Low", (index + 10), YesterdayKeyLevels.Low, VerticalAlignment.Top, HorizontalAlignment.Left, Colors.Black);
                ChartObjects.DrawText("DailyClose", "Daily Close", (index + 10), YesterdayKeyLevels.Close, VerticalAlignment.Top, HorizontalAlignment.Left, Colors.Black);


                ChartObjects.DrawText("DailyPivot", "Daily Pivot", (index - 10), P, VerticalAlignment.Top, HorizontalAlignment.Left, Colors.Gray);
              //  ChartObjects.DrawText("WeeklyPivot", "Weekly Pivot", (index - 10), WP, VerticalAlignment.Top, HorizontalAlignment.Left, Colors.Black);
               // ChartObjects.DrawText("MonthlyPivot", "Monthly Pivot", (index - 10), MP, VerticalAlignment.Top, HorizontalAlignment.Left, Colors.Black);


                // Print(P);
                ChartObjects.DrawText("R1", "R1", (index + 4), R1, VerticalAlignment.Top, HorizontalAlignment.Left, Colors.Red);
                ChartObjects.DrawText("R2", "R2", (index + 4), R2, VerticalAlignment.Top, HorizontalAlignment.Left, Colors.Red);
                ChartObjects.DrawText("R3", "R3", (index + 4), R3, VerticalAlignment.Top, HorizontalAlignment.Left, Colors.Red);

                ChartObjects.DrawText("S1", "S1", (index + 6), S1, VerticalAlignment.Top, HorizontalAlignment.Left, Colors.Green);
                ChartObjects.DrawText("S2", "S2", (index + 6), S2, VerticalAlignment.Top, HorizontalAlignment.Left, Colors.Green);
                ChartObjects.DrawText("S3", "S3", (index + 6), S3, VerticalAlignment.Top, HorizontalAlignment.Left, Colors.Green);


                ChartObjects.DrawText("CBOL", "CBOL", (index + 8), CBOL, VerticalAlignment.Top, HorizontalAlignment.Left, Colors.Yellow);
                ChartObjects.DrawText("CBOS", "CBOS", (index + 8), CBOS, VerticalAlignment.Top, HorizontalAlignment.Left, Colors.Yellow);

            }

            // WEEKLY
            //  ChartObjects.DrawText("WeeklyHigh", "Weekly High", (index - 10), WeeklyHigh, VerticalAlignment.Top, HorizontalAlignment.Left, Colors.Green);
            // ChartObjects.DrawText("WeeklyLow", "Weekly Low", (index - 20), WeeklyLow, VerticalAlignment.Top, HorizontalAlignment.Left, Colors.Black);
            //   ChartObjects.DrawText("WeeklyClose", "Weekly Close", (index - 30), WeeklyClose, VerticalAlignment.Top, HorizontalAlignment.Left, Colors.DeepSkyBlue);


            // MONTHLY
            // ChartObjects.DrawText("MonthlyHigh", "Monthly High", (index - 40), MonthlyHigh, VerticalAlignment.Top, HorizontalAlignment.Left, Colors.Green);
            // ChartObjects.DrawText("MonthlyLow", "Monthly Low", (index - 60), MonthlyLow, VerticalAlignment.Top, HorizontalAlignment.Left, Colors.Red);
            //   ChartObjects.DrawText("MonthlyClose", "Monthly Close", (index - 70), MonthlyClose, VerticalAlignment.Top, HorizontalAlignment.Left, Colors.DarkGray);



        }
    }



}
