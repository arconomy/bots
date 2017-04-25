using System;

namespace Niffler.Model
{ 
    public partial class KeyLevel
    {


        double DailyPivot = 0;
        double DailyR1 = 0;
        double DailyR2 = 0;
        double DailyR3 = 0;
        double DailyS1 = 0;
        double DailyS2 = 0;
        double DailyS3 = 0;
        double DailyCBOL = 0;
        double DailyCBOS = 0;
         
        double WeeklyPivot = 0;
        double MonthlyPivot = 0;

        public void CalculateDaily()
        {
             
            // Daily Levels

            DailyPivot  = (( High +  Low +  Close) / 3);

            DailyR1 = ((2 * DailyPivot) -  Low);
            DailyR2 = (DailyPivot +  High -  Low);
            DailyR3 = ( High + 2 * (DailyPivot -  Low));

            DailyS1 = ((2 * DailyPivot) -  High);
            DailyS2 = (DailyPivot -  High +  Low);
            DailyS3 =  Low - 2 * (High - DailyPivot);

            DailyCBOL = (( High -  Low) * 1.1 / 2 + Close);
            DailyCBOS =  Close - ( High -  Low) * 1.1 / 2;
             
            //WP = ((WeeklyHigh + WeeklyLow + WeeklyClose) / 3);
            //MP = ((MonthlyHigh + MonthlyLow + MonthlyClose) / 3);
             
        }

        public string BrokerName { get; set; }
         
        public string Symbol { get; set; }
         
        public DateTime Date { get; set; }
         
        public double High { get; set; }
         
        public double Low { get; set; }
         
        public double Close { get; set; }
         
        public double Open { get; set; }
    }
}
