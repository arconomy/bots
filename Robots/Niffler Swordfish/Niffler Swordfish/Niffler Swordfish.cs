using System;
using System.Linq;
using cAlgo.API;
using cAlgo.API.Indicators;
using cAlgo.API.Internals;
using cAlgo.Indicators;
using Niffler;


namespace cAlgo
{
    [Robot(TimeZone = TimeZones.UTC, AccessRights = AccessRights.FullAccess )]
    public class NifflerSwordfish : Niffler.Bots.Swordfish.Bot
    {
        [Parameter("Source")]
        public override DataSeries DataSeriesSource { get; set; }

        [Parameter("Use Bollinger Bollinger Band Entry", DefaultValue = false)]
        public override bool useBollingerBandEntry { get; set; }

        [Parameter("Pips inside Bollinger Band Entry", DefaultValue = 2)]
        public override int targetBolliEntryPips { get; set; }

        [Parameter("Initial Order placement trigger from open", DefaultValue = 5)]
        public override int SwordFishTrigger { get; set; }

        [Parameter("Offset from Market Open for First Order", DefaultValue = 9)]
        public override int OrderEntryOffset { get; set; }

        [Parameter("Distance between Orders in Pips", DefaultValue = 1)]
        public override int OrderSpacing { get; set; }

        [Parameter("# of Limit Orders", DefaultValue = 40)]
        public override int NumberOfOrders { get; set; }

        [Parameter("Volume (Lots)", DefaultValue = 1)]
        public override int Volume { get; set; }

        [Parameter("Volume Max (Lots)", DefaultValue = 200)]
        public override int VolumeMax { get; set; }

        [Parameter("# Order placed before Volume multiples", DefaultValue = 5)]
        public override int OrderVolumeLevels { get; set; }

        [Parameter("Volume multipler", DefaultValue = 2)]
        public override int VolumeMultipler { get; set; }

        [Parameter("Take Profit", DefaultValue = 0.5)]
        public override double TakeProfit { get; set; }

        [Parameter("Mins after swordfish period to reduce position risk", DefaultValue = 45)]
        public override int ReducePositionRiskTime { get; set; }

        [Parameter("Enable Retrace risk management", DefaultValue = true)]
        public override bool retraceEnabled { get; set; }

        [Parameter("Retrace level 1 Percentage", DefaultValue = 33)]
        public override int retraceLevel1 { get; set; }

        [Parameter("Retrace level 2 Percentage", DefaultValue = 50)]
        public override int retraceLevel2 { get; set; }

        [Parameter("Retrace level 3 Percentage", DefaultValue = 66)]
        public override int retraceLevel3 { get; set; }

        [Parameter("Initial Hard SL for last Order placed", DefaultValue = 5)]
        public override double FinalOrderStopLoss { get; set; }

        [Parameter("Triggered Hard SL buffer", DefaultValue = 20)]
        public override double HardStopLossBuffer { get; set; }

        [Parameter("Trailing SL fixed distance", DefaultValue = 5)]
        public override double TrailingStopPips { get; set; }
    }
}
