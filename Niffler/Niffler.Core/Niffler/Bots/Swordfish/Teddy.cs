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
        private StopLossManager StopLossManager;
        private MarketInfo SwfMarketInfo;
        private SellLimitOrdersTrader SellLimitOrdersTrader;
        private BuyLimitOrdersTrader BuyLimitOrdersTrader;
        private RulesManager RulesManager;

        protected override void OnStart()
        {
            Positions.Opened += OnPositionOpened;
            Positions.Closed += OnPositionClosed;
            BotState = new State(this);
            SwfMarketInfo = BotState.GetMarketInfo();
            SwfMarketInfo.SetCloseAfterMinutes(CloseAfterMins);
            SwfMarketInfo.SetReduceRiskAfterMinutes(ReduceRiskAfterMins);

            SellLimitOrdersTrader = new SellLimitOrdersTrader(BotState, NumberOfOrders, TriggerOrderPlacementPips, OrderEntryOffset, DefaultTakeProfit, FinalOrderStopLoss);
            BuyLimitOrdersTrader = new BuyLimitOrdersTrader(BotState, NumberOfOrders, TriggerOrderPlacementPips, OrderEntryOffset, DefaultTakeProfit, FinalOrderStopLoss);
            StopLossManager = new StopLossManager(BotState, HardStopLossBuffer, FinalOrderStopLoss);

            RulesManager = new RulesManager(BotState, SellLimitOrdersTrader, BuyLimitOrdersTrader, StopLossManager, new FixedTrailingStop(BotState, TrailingStopPips));

            RulesManager.SetOnTickRules(new List<IRule>
                {
                    new OpenTradingCapturePrice(1),
                    new OpenTradingCaptureSpike(2),
                    new OpenTradingPlaceLimitOrders(3),
                    new CloseTimeCancelPendingOrders(4),
                    new CloseTimeSetHardSLToLastPositionEntryWithBuffer(5),
                    new CloseTimeNoPositionsOpenedReset(6),
                    new CloseTimeNoPositionsRemainOpenReset(7),
                    new ReduceRiskTimeReduceRetraceLevels(8),
                    new ReduceRiskTimeSetHardSLToLastProfitPositionCloseWithBuffer(9),
                    new ReduceRiskTimeSetTrailingStop(10),
                    new RetracedLevel1To2SetHardSLToLastProfitPositionEntryWithBuffer(11),
                    new RetracedLevel1To2SetBreakEvenSLActive(12),
                    new RetracedLevel2To3SetHardSLToLastProfitPositionEntry(13),
                    new RetracedLevel2To3SetHardSLToLastProfitPositionEntry(14),
                    new RetracedLevel3PlusReduceHardSLBuffer(15),
                    new RetracedLevel3PlusSetHardSLToLastProfitPositionCloseWithBuffer(16),
                    new OnTickBreakEvenSLActiveSetLastProfitPositionEntry(17),
                    new OnTickTrailingStopActiveSetFixedTrailingSL(18),
                    new OnTickTrailingActiveChase(19),
                    new TerminateTimeCloseAllPositionsReset(20)
            });

            RulesManager.SetOnPositionOpenedRules(new List<IRuleOnPositionEvent>
                {
                    new OnPositionOpenedCaptureLastPositionInfo(1)
            });

            RulesManager.SetOnPositionClosedRules(new List<IRuleOnPositionEvent>
                {
                    new OnPositionClosedReportTrade(1),
                    new OnPositionClosedLastEntryPositionStopLossTriggeredCloseAll(2),
                    new OnPositionClosedInProfitSetBreakEvenWithBufferIfActive(3),
                    new OnPositionClosedInProfitCaptureProfitPositionInfo(4)
            });
        }

        protected override void OnTick()
        {
            RulesManager.OnTick();
        }

        protected void OnPositionOpened(PositionOpenedEventArgs args)
        {
            RulesManager.OnPositionOpened(args.Position);
        }

        protected void OnPositionClosed(PositionClosedEventArgs args)
        {
            RulesManager.OnPositionClosed(args.Position);
        }

        protected override void OnStop()
        {
        // Put your deinitialization logic here

        }

    }
}


    





















