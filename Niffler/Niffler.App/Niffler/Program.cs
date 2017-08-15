using Niffler.Common;
using Niffler.Common.Market;
using Niffler.Common.Trade;
using Niffler.Messaging.RabbitMQ;
using Niffler.Rules;
using RabbitMQ.Client;

namespace Niffler.App
{
    class Program
    {
        private State BotState;
        private StopLossManager StopLossManager;
        private MarketInfo SwfMarketInfo;
        private SellLimitOrdersTrader SellLimitOrdersTrader;
        private BuyLimitOrdersTrader BuyLimitOrdersTrader;
        private RulesManager RulesManager;

        public static void Main()
        {

            //Set up Micro-services...
            RulesManager = new RulesManager();

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


        //Set up Messaging..
        var adapter = Adapter.Instance;
            adapter.Init("localhost", "nifflermq", 15672, "niffler", "niffler", 50);
            adapter.Connect();
       
            var lifeCycleEventsConsumer = ConsumerFactory.CreateConsumer((IConnection) adapter.GetConnection(), "FTSE100X", "topic", "LifeCyleEventsQ",new string[]{"OnStart.*","OnStop.*"}, 10);
            adapter.ConsumeAsync(lifeCycleEventsConsumer);

            var positionsOpenedConsumer = ConsumerFactory.CreateConsumer((IConnection)adapter.GetConnection(), "FTSE100X", "topic", "PositionsOpenedQ", new string[] { "OnPositionOpened.*" }, 10);
            adapter.ConsumeAsync(positionsOpenedConsumer);

            var positionsClosedConsumer = ConsumerFactory.CreateConsumer((IConnection)adapter.GetConnection(), "FTSE100X", "topic", "PositionsClosedQ", new string[] { "OnPositionClosed.*" }, 10);
            adapter.ConsumeAsync(positionsClosedConsumer);

            var ticksConsumer = ConsumerFactory.CreateConsumer((IConnection)adapter.GetConnection(), "FTSE100X", "topic", "TicksQ", new string[] { "OnTick.*" }, 10);
            adapter.ConsumeAsync(ticksConsumer);

            Console.WriteLine(" Press [enter] to exit.");
            Console.ReadLine();
        }
    }


}
