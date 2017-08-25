using System.Collections.Generic;
using System.Linq;
using Niffler.Common;
using Niffler.Common.Trade;
using Niffler.Common.Market;
using Niffler.Common.TrailingStop;
using cAlgo.API;
using Niffler.Common.BackTest;
using Niffler.Messaging;
using System;
using Niffler.Rules;
using Niffler.Strategy;
using RabbitMQ.Client;
using Niffler.Messaging.RabbitMQ;

namespace Niffler.Microservices
{
    class ServicesManager : IResetState
    {
        public SellLimitOrdersTrader SellLimitOrdersTrader { get; }
        public BuyLimitOrdersTrader BuyLimitOrdersTrader { get; }
        public PositionsManager PositionsManager { get; }
        public SpikeManager SpikeManager { get; }
        public StopLossManager StopLossManager { get; }
        public FixedTrailingStop FixedTrailingStop { get; }
        public StateManager StateManager { get; }
        private Reporter Reporter { get; set; }
        private List<IRule> OnTickRules = new List<IRule>();
        private List<IPositionRule> OnPositionOpenedRules = new List<IPositionRule>();
        private List<IPositionRule> OnPositionClosedRules = new List<IPositionRule>();
        private List<IRule> OnTimerRules = new List<IRule>();
        private List<IRule> OnBarRules = new List<IRule>();

        private StopLossManager StopLossManager;
        private TimeInfo SwfMarketInfo;
        private SellLimitOrdersTrader SellLimitOrdersTrader;
        private BuyLimitOrdersTrader BuyLimitOrdersTrader;

        private RulesFactory RulesFactory = new RulesFactory();
        private List<IRule> Rules;

        private List<StateManager> StateManagers;

        public IConnection Connection;

        public ServicesManager(IConnection connection, StrategyConfig strategyConfig)
        {

            this.Connection = connection;


            //Create default microservices
            SpikeManager = new SpikeManager();
            Reporter = new Reporter();

            //For each BotConfig Initialise a micro-service for each service required and listen for updates on appropriate queues
            foreach (BotConfig botConfig in strategyConfig.BotConfig)
            {
                // Need to refactor StateManager to manage a State object to store persistent state info
                // All data used for rules is passed to the ruleService when intatiated. i.e. Open time etc.
                // Once timing rules have fired the state will need to notified i.e. IsTrading = true.

                //Generate Strategy ID here and pass to the State and Rules
                botConfig.Config.Add("StategyId", GenerateStrategyId());

                //Create a State Manager per bot
                StateManager = new StateManager(botConfig.Config);
                StateManager.Start();
                StateManagers.Add(StateManager);
                Rules = RulesFactory.CreateRules(botConfig);

            }


            Reporter = botState.GetReporter();
            SellLimitOrdersTrader = sellLimitOrdersTrader;
            BuyLimitOrdersTrader = buyLimitOrdersTrader;
            PositionsManager = new PositionsManager(StateManager);
            SpikeManager = new SpikeManager(StateManager);
            StopLossManager = stopLossManager;
            FixedTrailingStop = fixedTrailingStop;
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

        //RulesManager.SetOnPositionOpenedRules(new List<IRuleOnPositionEvent>
        //    {
        //        new OnPositionOpenedCaptureLastPositionInfo(1)
        //});

        //RulesManager.SetOnPositionClosedRules(new List<IRuleOnPositionEvent>
        //    {
        //        new OnPositionClosedReportTrade(1),
        //        new OnPositionClosedLastEntryPositionStopLossTriggeredCloseAll(2),
        //        new OnPositionClosedInProfitSetBreakEvenWithBufferIfActive(3),
        //        new OnPositionClosedInProfitCaptureProfitPositionInfo(4)
        //});
        // }


        public void AddOnTickRule(IRule rule)
        {
            rule.Init(this);
            OnTickRules.Add(rule);
            OnTickRules = OnTickRules.OrderBy(r => r.Priority).ToList();
        }

        public void AddOnBarRule(IRule rule)
        {
            rule.Init(this);
            OnBarRules.Add(rule);
            OnBarRules = OnBarRules.OrderBy(r => r.Priority).ToList();
        }

        public void SetOnTimerRule(IRule rule)
        {
            rule.Init(this);
            OnTimerRules.Add(rule);
            OnTimerRules = OnTimerRules.OrderBy(r => r.Priority).ToList();
        }

        public void SetOnPositionClosedRule(IPositionRule rule)
        {
            rule.Init(this);
            OnPositionClosedRules.Add(rule);
            OnPositionClosedRules = OnPositionClosedRules.OrderBy(r => r.Priority).ToList();
        }

        public void SetOnPositionOpenedRule(IPositionRule rule)
        {
            rule.Init(this);
            OnPositionOpenedRules.Add(rule);
            OnPositionOpenedRules = OnPositionOpenedRules.OrderBy(r => r.Priority).ToList();
        }

        public void SetOnTickRules(List<IRule> rules)
        {
            InitialiseRules(rules);
            OnTickRules = rules.OrderBy(rule => rule.Priority).ToList();
        }

        public void SetOnBarRules(List<IRule> rules)
        {
            InitialiseRules(rules);
            OnBarRules = rules.OrderBy(rule => rule.Priority).ToList();
        }

        public void SetOnTimerRules(List<IRule> rules)
        {
            InitialiseRules(rules);
            OnTimerRules = (rules.OrderBy(rule => rule.Priority).ToList());
        }

        public void SetOnPositionClosedRules(List<IPositionRule> rules)
        {
            InitialiseRules(rules.ConvertAll(x => (IRule)x));
            OnPositionClosedRules = rules.OrderBy(rule => rule.Priority).ToList();
        }

        public void SetOnPositionOpenedRules(List<IPositionRule> rules)
        {
            InitialiseRules(rules.ConvertAll(x => (IRule)x));
            OnPositionOpenedRules = rules.OrderBy(rule => rule.Priority).ToList();
        }

        private void InitialiseRules(List<IRule> rules)
        {
            rules.ForEach(rule => rule.Init(this));
        }

        public void OnTick()
        {
            //Run the onTick Rules
            RunAllRules(OnTickRules);
        }

        //Run the onBar Rules
        public void OnBar()
        {
            RunAllRules(OnBarRules);
        }

        public void OnPositionClosed(Position position)
        {
            RunAllPositionRules(OnPositionClosedRules, position);
        }

        public void OnPositionOpened(Position position)
        {
            RunAllPositionRules(OnPositionOpenedRules, position);
        }

        public void OnTimer()
        {
            RunAllRules(OnTimerRules);
        }

        public void ExecuteRule(IRule rule)
        {
            rule.Run();
        }

        public void RunAllPositionRules(List<IPositionRule> rules,Position position)
        {
            rules.ForEach(IRuleOnPositionEvent => IRuleOnPositionEvent.Run(position));
        }

        public void RunAllRules(List<IRule> rules)
        {
            rules.ForEach(IRule => IRule.Run());
        }

        public void ResetRules()
        {
            OnTickRules.ForEach(IRule => IRule.Reset());
            OnBarRules.ForEach(IRule => IRule.Reset());
            OnTimerRules.ForEach(IRule => IRule.Reset());
            OnPositionClosedRules.ForEach(IRule => IRule.Reset());
            OnPositionOpenedRules.ForEach(IRule => IRule.Reset());
        }

        public void Reset()
        {
                ReportResults();
                ResetRules();
                SpikeManager.Reset();
                FixedTrailingStop.Reset();
                Reporter.Reset();
        }

        //Report Summary Rules Execution Results
        private void ReportResults()
        {
            OnTickRules.ForEach(IRule => IRule.ReportExecutionCount());
            OnBarRules.ForEach(IRule => IRule.ReportExecutionCount());
            OnTimerRules.ForEach(IRule => IRule.ReportExecutionCount());
            OnPositionOpenedRules.ForEach(IRule => IRule.ReportExecutionCount());
            OnPositionClosedRules.ForEach(IRule => IRule.ReportExecutionCount());
            StateManager.GetReporter().Report();
        }


    }
}
