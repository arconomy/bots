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

        virtual public DataSeries DataSeriesSource { get; set; }
        virtual public bool UseBollingerBandEntry { get; set; }
        virtual public int BolliEntryPips { get; set; }
        virtual public int TriggerOrderPlacementPips { get; set; }
        virtual public int OrderEntryOffset { get; set; }
        virtual public int OrderSpacing { get; set; }
        virtual public int OrderSpacingLevels { get; set; }
        virtual public double OrderSpacingMultipler { get; set; }
        virtual public int OrderSpacingMax { get; set; }
        virtual public int NumberOfOrders { get; set; }
        virtual public int Volume { get; set; }
        virtual public int VolumeMax { get; set; }
        virtual public int OrderVolumeLevels { get; set; }
        virtual public double VolumeMultipler { get; set; }
        virtual public double DefaultTakeProfit { get; set; }
        virtual public int ReduceRiskAfterMins { get; set; }
        virtual public int CloseAfterMins { get; set; }
        virtual public int RetraceLevel1 { get; set; }
        virtual public int RetraceLevel2 { get; set; }
        virtual public int RetraceLevel3 { get; set; }
        virtual public double FinalOrderStopLoss { get; set; }
        virtual public double HardStopLossBuffer { get; set; }
        virtual public double TrailingStopPips { get; set; }

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


    





















