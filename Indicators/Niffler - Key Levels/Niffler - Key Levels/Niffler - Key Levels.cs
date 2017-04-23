using System;
using cAlgo.API;
using cAlgo.API.Internals;
using cAlgo.API.Indicators;
using cAlgo.Indicators;
using Niffler;

namespace cAlgo
{
    [Indicator(IsOverlay = false, TimeZone = TimeZones.UTC, AccessRights = AccessRights.None)]
    public class NifflerKeyLevels : Niffler.Indicators.KeyLevels.Bot
    {


        // Niffler Key Levels

        // Used to put Key lines on your charts. Requires the Client Bot to be running to collect the Cash hour Data

        // Enjoy!


        [Parameter("Source")]
        public override DataSeries Source { get; set; }

        [Parameter("Yesterdays High")]
        public override double CashHigh { get; set; }

        [Parameter("Yesterdays Low")]
        public override double CashLow { get; set; }

        [Parameter("Yesterdays Close")]
        public override double CashClose { get; set; }

        [Parameter("Yesterdays Open")]
        public override double CashOpen { get; set; }

        [Parameter("Daily ATR")]
        public override double CashATR { get; set; }

        [Parameter("Weekly High")]
        public override double WeeklyHigh { get; set; }

        [Parameter("Weekly Low")]
        public override double WeeklyLow { get; set; }

        [Parameter("Weekly Close")]
        public override double WeeklyClose { get; set; }

        [Parameter("Monthly High")]
        public override double MonthlyHigh { get; set; }

        [Parameter("Monthly Low")]
        public override double MonthlyLow { get; set; }

        [Parameter("Monthly Close")]
        public override double MonthlyClose { get; set; }


    }
}
