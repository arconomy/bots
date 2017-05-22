using System;
using System.Collections.Generic;
using cAlgo.API;
using cAlgo.API.Internals;
using cAlgo.API.Indicators;
using Niffler.Common;
using Niffler.Common.TrailingStop;
using Niffler.Common.Market;
using Niffler.Common.Trade;
using Niffler.Common.BackTest;
using Niffler.Rules;

namespace Niffler.Bots.Swordfish

{
    public class Teddy : cAlgo.API.Robot
    { 
        [Parameter("Source")]
        public DataSeries DataSeriesSource { get; set; }

        [Parameter("Use Bollinger Bollinger Band Entry", DefaultValue = false)]
        public bool UseBollingerBandEntry { get; set; }

        [Parameter("Pips inside Bollinger Band for Entry", DefaultValue = 2)]
        public int BolliEntryPips { get; set; }

        [Parameter("Initial Order placement trigger from open", DefaultValue = 5)]
        public int TriggerOrderPlacementPips { get; set; }

        [Parameter("Offset from Market Open for First Order", DefaultValue = 9)]
        public int OrderEntryOffset { get; set; }

        [Parameter("Order spacing in Pips", DefaultValue = 1)]
        public int OrderSpacing { get; set; }

        [Parameter("# Order placed before order spacing multiplies", DefaultValue = 10)]
        public int OrderSpacingLevels { get; set; }

        [Parameter("Order spacing multipler", DefaultValue = 2)]
        public double OrderSpacingMultipler { get; set; }

        [Parameter("Order spacing max", DefaultValue = 3)]
        public int OrderSpacingMax { get; set; }

        [Parameter("# of Limit Orders", DefaultValue = 40)]
        public int NumberOfOrders { get; set; }

        [Parameter("Volume (Lots)", DefaultValue = 1)]
        public int Volume { get; set; }

        [Parameter("Volume Max (Lots)", DefaultValue = 200)]
        public int VolumeMax { get; set; }

        [Parameter("# Order placed before Volume multiplies", DefaultValue = 5)]
        public int OrderVolumeLevels { get; set; }

        [Parameter("Volume multipler", DefaultValue = 2)]
        public double VolumeMultipler { get; set; }

        [Parameter("Take Profit", DefaultValue = 0.5)]
        public double DefaultTakeProfit { get; set; }

        [Parameter("Mins after swordfish period to reduce position risk", DefaultValue = 45)]
        public int ReduceRiskAfterMins { get; set; }

        [Parameter("Mins after swordfish period to reduce position risk", DefaultValue = 45)]
        public int CloseAfterMins { get; set; }

        [Parameter("Retrace level 1 Percentage", DefaultValue = 33)]
        public int RetraceLevel1 { get; set; }

        [Parameter("Retrace level 2 Percentage", DefaultValue = 50)]
        public int RetraceLevel2 { get; set; }

        [Parameter("Retrace level 3 Percentage", DefaultValue = 66)]
        public int RetraceLevel3 { get; set; }

        [Parameter("Initial Hard SL for last Order placed", DefaultValue = 5)]
        public double FinalOrderStopLoss { get; set; }

        [Parameter("Triggered Hard SL buffer", DefaultValue = 20)]
        public double HardStopLossBuffer { get; set; }

        [Parameter("Trailing SL fixed distance", DefaultValue = 5)]
        public double TrailingStopPips { get; set; }

        private State BotState;
        private FixedTrailingStop TrailingStop;
        private StopLossManager StopLossManager;
        private SpikeManager SpikeManager;
        private MarketInfo SwfMarketInfo;
        private SellLimitOrdersTrader SellLimitOrdersTrader;
        private BuyLimitOrdersTrader BuyLimitOrdersTrader;
        private RulesManager RulesManager;

        protected override void OnStart()
        {
            Positions.Opened += OnPositionOpened;
            Positions.Closed += OnPositionClosed;
            BotState = new State(this);
            SwfMarketInfo = BotState.getMarketInfo();
            SwfMarketInfo.SetCloseAfterMinutes(CloseAfterMins);
            SwfMarketInfo.SetReduceRiskAfterMinutes(ReduceRiskAfterMins);

            SellLimitOrdersTrader = new SellLimitOrdersTrader(BotState, NumberOfOrders, TriggerOrderPlacementPips, OrderEntryOffset, DefaultTakeProfit, FinalOrderStopLoss);
            BuyLimitOrdersTrader = new BuyLimitOrdersTrader(BotState, NumberOfOrders, TriggerOrderPlacementPips, OrderEntryOffset, DefaultTakeProfit, FinalOrderStopLoss);
            StopLossManager = new StopLossManager(BotState, HardStopLossBuffer, FinalOrderStopLoss);

            RulesManager = new RulesManager(BotState, SellLimitOrdersTrader, BuyLimitOrdersTrader, SpikeManager, StopLossManager, new FixedTrailingStop(BotState, TrailingStopPips));

            RulesManager.setOnTickRules(new List<IRule>
                {
                    new OpenTradingCapturePrice(1),
                    new OpenTradingCaptureSpike(2),
                    new OpenTradingPlaceLimitOrders(2),
                    new CloseTimeCancelPendingOrders(3),
                    new CloseTimeSetHardSLToLastPositionEntryWithBuffer(4),
                    new CloseTimeNoPositionsOpenedReset(5),
                    new CloseTimeNoPositionsRemainOpenReset(5),
                    new ReduceRiskTimeReduceRetraceLevels(6),
                    new ReduceRiskTimeSetHardSLToLastProfitPositionCloseWithBuffer(7),
                    new ReduceRiskTimeSetTrailingStop(8),
                    new RetracedLevel1To2SetHardSLToLastProfitPositionEntryWithBuffer(9),
                    new RetracedLevel1To2SetBreakEvenSLActive(10),
                    new RetracedLevel2To3SetHardSLToLastProfitPositionEntry(11),
                    new RetracedLevel2To3SetHardSLToLastProfitPositionEntry(12),
                    new RetracedLevel3PlusReduceHardSLBuffer(13),
                    new RetracedLevel3PlusSetHardSLToLastProfitPositionCloseWithBuffer(14),
                    new OnTickBreakEvenSLActiveSetLastProfitPositionEntry(15),
                    new OnTickTrailingStopActiveSetFixedTrailingSL(16),
                    new OnTickTrailingActiveChase(17),
                    new TerminateTimeCloseAllPositionsReset(17)
            });

            RulesManager.setOnPositionOpenedRules(new List<IRuleOnPositionEvent>
                {
                    new OnPositionOpenedCaptureLastPositionInfo(1)
            });

            RulesManager.setOnPositionClosedRules(new List<IRuleOnPositionEvent>
                {
                    new OnPositionClosedReportTrade(1),
                    new OnPositionClosedLastEntryPositionStopLossTriggeredCloseAll(2),
                    new OnPositionClosedInProfitSetBreakEvenWithBufferIfActive(3),
                    new OnPositionClosedInProfitCaptureProfitPositionInfo(4)
            });
        }

        protected override void OnTick()
        {
            RulesManager.onTick();
        }

        protected void OnPositionOpened(PositionOpenedEventArgs args)
        {
            RulesManager.onPositionOpened(args.Position);
        }

        protected void OnPositionClosed(PositionClosedEventArgs args)
        {
            RulesManager.onPositionClosed(args.Position);
        }

        protected override void OnStop()
        {
        // Put your deinitialization logic here

        }

    }
}


    





















