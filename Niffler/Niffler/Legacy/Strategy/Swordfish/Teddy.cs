using System.Collections.Generic;
using cAlgo.API;
using Niffler.Common;
using Niffler.Common.TrailingStop;
using Niffler.Common.Market;
using Niffler.Common.Trade;
using Niffler.Rules;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;
using System;
using System.Text;
using System.Threading.Tasks;
using Niffler.Messaging.AMQP;

namespace Niffler.Bots.Swordfish

{
    public class Teddy
    {
    }
}

        //virtual public DataSeries DataSeriesSource { get; set; }
        //virtual public bool UseBollingerBandEntry { get; set; }
        //virtual public int BolliEntryPips { get; set; }
        //virtual public int TriggerOrderPlacementPips { get; set; }
        //virtual public int OrderEntryOffset { get; set; }
        //virtual public int OrderSpacing { get; set; }
        //virtual public int OrderSpacingLevels { get; set; }
        //virtual public double OrderSpacingMultipler { get; set; }
        //virtual public int OrderSpacingMax { get; set; }
        //virtual public int NumberOfOrders { get; set; }
        //virtual public int VolumeBase { get; set; }
        //virtual public int VolumeMax { get; set; }
        //virtual public int VolumeMultiplierOrderLevels { get; set; }
        //virtual public double VolumeMultipler { get; set; }
        //virtual public double DefaultTakeProfit { get; set; }
        //virtual public int ReduceRiskAfterMins { get; set; }
        //virtual public int CloseAfterMins { get; set; }
        //virtual public int RetraceLevel1 { get; set; }
        //virtual public int RetraceLevel2 { get; set; }
        //virtual public int RetraceLevel3 { get; set; }
        //virtual public double FinalOrderStopLoss { get; set; }
        //virtual public double HardStopLossBuffer { get; set; }
        //virtual public double TrailingStopPips { get; set; }

        //private State BotState;
        //private StopLossManager StopLossManager;
        //private MarketInfo SwfMarketInfo;
        //private SellLimitOrdersTrader SellLimitOrdersTrader;
        //private BuyLimitOrdersTrader BuyLimitOrdersTrader;
        //private RulesManager RulesManager;
        
        /*

        protected void OnStart()
        {
            Positions.Opened += OnPositionOpened;
            Positions.Closed += OnPositionClosed;
            BotState = new State(this);
            SwfMarketInfo = BotState.GetMarketInfo();
            SwfMarketInfo.SetCloseAfterMinutes(CloseAfterMins);
            SwfMarketInfo.SetReduceRiskAfterMinutes(ReduceRiskAfterMins);

            SellLimitOrdersTrader = new SellLimitOrdersTrader(BotState, NumberOfOrders, TriggerOrderPlacementPips, OrderEntryOffset, DefaultTakeProfit, FinalOrderStopLoss);
            SellLimitOrdersTrader.SetVolumeMultipler(VolumeMultiplierOrderLevels, VolumeMultipler, VolumeMax, VolumeBase);
            BuyLimitOrdersTrader = new BuyLimitOrdersTrader(BotState, NumberOfOrders, TriggerOrderPlacementPips, OrderEntryOffset, DefaultTakeProfit, FinalOrderStopLoss);
            BuyLimitOrdersTrader.SetVolumeMultipler(VolumeMultiplierOrderLevels, VolumeMultipler, VolumeMax, VolumeBase);
            StopLossManager = new StopLossManager(BotState, HardStopLossBuffer, FinalOrderStopLoss);

            RulesManager = new RulesManager(BotState, SellLimitOrdersTrader, BuyLimitOrdersTrader, StopLossManager, new FixedTrailingStop(BotState, TrailingStopPips));

            RulesManager.SetOnTickRules(new List<IRule>
                {
                    new OpenTimeSetBotState(1),
                 //   new OpenTimeCapturePrice(2),
                 //   new OpenTimeCaptureSpike(3),
                 //   new OpenTimePlaceLimitOrders(4),
                 //   new CloseTimeSetBotState(5),
                 //   new CloseTimeCancelPendingOrders(6),
                 //   new CloseTimeSetHardSLToLastPositionEntryWithBuffer(7),
                 //   new CloseTimeNoPositionsOpenedReset(8),
                 //   new CloseTimeNoPositionsRemainOpenReset(9),
                 //   new ReduceRiskTimeSetBotState(10),
                 //   new ReduceRiskTimeReduceRetraceLevels(11),
                 //   new ReduceRiskTimeSetHardSLToLastProfitPositionCloseWithBuffer(12),
                 //   new ReduceRiskTimeSetTrailingStop(13),
                 //   new TerminateTimeSetBotState(14),
                 //   new TerminateTimeCloseAllPositionsReset(15),
                 //   new RetracedLevel1To2SetHardSLToLastProfitPositionEntryWithBuffer(16),
                 //   new RetracedLevel1To2SetBreakEvenSLActive(17),
                 //   new RetracedLevel2To3SetHardSLToLastProfitPositionEntry(18),
                 //   new RetracedLevel2To3SetHardSLToLastProfitPositionEntry(19),
                //    new RetracedLevel3PlusReduceHardSLBuffer(20),
                 //   new RetracedLevel3PlusSetHardSLToLastProfitPositionCloseWithBuffer(21),
                //    new OnTickBreakEvenSLActiveSetLastProfitPositionEntry(22),
                //    new OnTickChaseFixedTrailingSL(23),
                //    new StartTrading(24), //Start Trading is based on flags set by the other non-trading rules therefore run afer all other rules
                //    new EndTrading(25), //End Trading is based on flags set by the other trading rules therefore run afer all other rules
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
*/
























