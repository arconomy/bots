using System.Collections.Generic;
using System;
using Niffler.Rules;
using Niffler.Messaging.RabbitMQ;
using Niffler.Core.Strategy;

namespace Niffler.Services
{
    public class ServicesManager : IScalableConsumerService
    {
        private RulesFactory RulesFactory = new RulesFactory();
        private List<IRule> Rules;
        private List<IScalableConsumerService> Services;
        private AppConfiguration AppConfig;

        public ServicesManager(AppConfiguration appConfig)
        {
            AppConfig = appConfig;
        }

        public override void Init()
        {
            //Set up Adapter to manage single connection for all consumers.
            Adapter = Adapter.Instance;
            Adapter.Init();
            Adapter.Connect();

            //For each StrategyConfig Initialise Rules as micro-services and other default micro-services
            foreach (StrategyConfiguration strategyConfig in AppConfig.StrategyConfigList)
            {
                //Generate Strategy ID here and pass to the State and Rules
                strategyConfig.Config.Add("StategyId", GenerateStrategyId());

                //Create the rule services per strategy
                Rules = (RulesFactory.CreateAndInitRules(strategyConfig));
                Rules.ForEach(rule => rule.Run(Adapter));

                //Create a State Manager per strategy
                Services.Add(new StateManager(strategyConfig));

                //Create a Report Manager per strategy
                Services.Add(new ReportManager(strategyConfig));

                Services.ForEach(service => service.Init());
                Services.ForEach(service => service.Run(Adapter));
            }
            //TO DO: Set up QueueWatchers to autoscale consumers. Autoscale message is a Service Message with a routingkey = QueueName on the AutoScaleX exchange.
        }

        public override void ShutDown()
        {
            ShutDownConsumers();
            Rules.ForEach(rule => rule.ShutDown());
            Services.ForEach(service => service.ShutDown());
        }

        public override void Reset()
        {
            Rules.ForEach(rule => rule.Reset());
            Services.ForEach(service => service.Reset());
        }

        protected override List<RoutingKey> SetListeningRoutingKeys()
        {
            //Need to implement queue monitoring and listen for updates for busy queues
            return null;
        }

        protected override void OnMessageReceived(object sender, MessageReceivedEventArgs e)
        {
            //Need to implement queue monitoring and listen for updates for busy queues
            throw new NotImplementedException();
        }

        static private string GenerateStrategyId()
        {
            Random randomIdGenerator = new Random();
            int id = randomIdGenerator.Next(0, 99999);
            return id.ToString("00000");
        }


        //

        //BotState = new State(this);
        //SwfMarketInfo = BotState.GetMarketInfo();
        //SwfMarketInfo.SetCloseAfterMinutes(CloseAfterMins);
        //SwfMarketInfo.SetReduceRiskAfterMinutes(ReduceRiskAfterMins);

        //SellLimitOrdersTrader = new SellLimitOrdersTrader(BotState, NumberOfOrders, TriggerOrderPlacementPips, OrderEntryOffset, DefaultTakeProfit, FinalOrderStopLoss);
        //SellLimitOrdersTrader.SetVolumeMultipler(VolumeMultiplierOrderLevels, VolumeMultipler, VolumeMax, VolumeBase);
        //BuyLimitOrdersTrader = new BuyLimitOrdersTrader(BotState, NumberOfOrders, TriggerOrderPlacementPips, OrderEntryOffset, DefaultTakeProfit, FinalOrderStopLoss);
        //BuyLimitOrdersTrader.SetVolumeMultipler(VolumeMultiplierOrderLevels, VolumeMultipler, VolumeMax, VolumeBase);
        //StopLossManager = new StopLossManager(BotState, HardStopLossBuffer, FinalOrderStopLoss);

        //RulesManager = new RulesManager(BotState, SellLimitOrdersTrader, BuyLimitOrdersTrader, StopLossManager, new FixedTrailingStop(BotState, TrailingStopPips));

        //RulesManager.SetOnTickRules(new List<IRule>
        //    {
        //        new OpenTimeSetBotState(1),
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
        //});
        
    }
}
