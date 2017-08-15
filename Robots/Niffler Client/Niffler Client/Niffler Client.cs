using System;
using System.Linq;
using cAlgo.API;
using cAlgo.API.Indicators;
using cAlgo.API.Internals;
using cAlgo.Indicators;
using NifflerClient;


namespace cAlgo
{
    [Robot(TimeZone = TimeZones.UTC, AccessRights = AccessRights.FullAccess)]
    public class NifflerClient : NifflerClient.Botty
    {
        [Parameter("Source")]
        public override DataSeries DataSeriesSource { get; set; }

        [Parameter("Use Bollinger Bollinger Band Entry", DefaultValue = false)]
        public override bool UseBollingerBandEntry { get; set; }

        [Parameter("Pips inside Bollinger Band for Entry", DefaultValue = 2)]
        public override int BolliEntryPips { get; set; }

        [Parameter("Initial Order placement trigger from open", DefaultValue = 5)]
        public override int TriggerOrderPlacementPips { get; set; }

        [Parameter("Offset from Market Open for First Order", DefaultValue = 9)]
        public override int OrderEntryOffset { get; set; }

        [Parameter("Order spacing in Pips", DefaultValue = 1)]
        public override int OrderSpacing { get; set; }

        [Parameter("# Order placed before order spacing multiplies", DefaultValue = 10)]
        public override int OrderSpacingLevels { get; set; }

        [Parameter("Order spacing multipler", DefaultValue = 2)]
        public override double OrderSpacingMultipler { get; set; }

        [Parameter("Order spacing max", DefaultValue = 3)]
        public override int OrderSpacingMax { get; set; }

        [Parameter("# of Limit Orders", DefaultValue = 40)]
        public override int NumberOfOrders { get; set; }

        [Parameter("Volume (Lots)", DefaultValue = 1)]
        public override int VolumeBase { get; set; }

        [Parameter("Volume Max (Lots)", DefaultValue = 200)]
        public override int VolumeMax { get; set; }

        [Parameter("# Order placed before Volume multiplies", DefaultValue = 5)]
        public override int VolumeMultiplierOrderLevels { get; set; }

        [Parameter("Volume multipler", DefaultValue = 2)]
        public override double VolumeMultipler { get; set; }

        [Parameter("Take Profit", DefaultValue = 0.5)]
        public override double DefaultTakeProfit { get; set; }

        [Parameter("Mins after market open to reduce position risk", DefaultValue = 45)]
        public override int ReduceRiskAfterMins { get; set; }

        [Parameter("Mins after market open to stop swordfish trading", DefaultValue = 45)]
        public override int CloseAfterMins { get; set; }

        [Parameter("Retrace level 1 Percentage", DefaultValue = 33)]
        public override int RetraceLevel1 { get; set; }

        [Parameter("Retrace level 2 Percentage", DefaultValue = 50)]
        public override int RetraceLevel2 { get; set; }

        [Parameter("Retrace level 3 Percentage", DefaultValue = 66)]
        public override int RetraceLevel3 { get; set; }

        [Parameter("Initial Hard SL for last Order placed", DefaultValue = 5)]
        public override double FinalOrderStopLoss { get; set; }

        [Parameter("Triggered Hard SL buffer", DefaultValue = 20)]
        public override double HardStopLossBuffer { get; set; }

        [Parameter("Trailing SL fixed distance", DefaultValue = 5)]
        public override double TrailingStopPips { get; set; }
    }
}
