using System;
using cAlgo.API;
using cAlgo.API.Internals;
using cAlgo.API.Indicators;
using cAlgo.Indicators;
using Niffler;

namespace cAlgo
{
    [Indicator(IsOverlay = true, TimeZone = TimeZones.UTC, AccessRights = AccessRights.FullAccess)]
    public class NifflerKeyLevels : Niffler.Indicators.KeyLevels.Bot
    {


        // Niffler Key Levels

        // Used to put Key lines on your charts. Requires the Client Bot to be running to collect the Cash hour Data

        // Enjoy!


        [Output("Daily Open", Color = Colors.Green)]
        public override IndicatorDataSeries DailyOpenSeries { get; set; }

        [Output("Daily Close", Color = Colors.Red)]
        public override IndicatorDataSeries DailyCloseSeries { get; set; }

        [Output("Daily High", Color = Colors.Lime)]
        public override IndicatorDataSeries DailyHighSeries { get; set; }

        [Output("Daily Low", Color = Colors.Lime)]
        public override IndicatorDataSeries DailyLowSeries { get; set; }


        [Output("Daily R1", Color = Colors.Red)]
        public override IndicatorDataSeries DailyR1Series { get; set; }

        [Output("Daily R2", Color = Colors.Red)]
        public override IndicatorDataSeries DailyR2Series { get; set; }

        [Output("Daily R3", Color = Colors.Red)]
        public override IndicatorDataSeries DailyR3Series { get; set; }


        [Output("Daily S1", Color = Colors.Green)]
        public override IndicatorDataSeries DailyS1Series { get; set; }

        [Output("Daily S2", Color = Colors.Green)]
        public override IndicatorDataSeries DailyS2Series { get; set; }

        [Output("Daily S3", Color = Colors.Green)]
        public override IndicatorDataSeries DailyS3Series { get; set; }


        [Output("Daily CBOL", Color = Colors.Yellow, LineStyle = LineStyle.LinesDots)]
        public override IndicatorDataSeries DailyCBOLSeries { get; set; }

        [Output("Daily CBOS", Color = Colors.Yellow, LineStyle = LineStyle.LinesDots)]
        public override IndicatorDataSeries DailyCBOSSeries { get; set; }

        [Output("Daily Pivot", Color = Colors.Gray)]
        public override IndicatorDataSeries DailyPivotSeries { get; set; }

        [Parameter("Source")]
        public override DataSeries Source { get; set; }


    }
}
