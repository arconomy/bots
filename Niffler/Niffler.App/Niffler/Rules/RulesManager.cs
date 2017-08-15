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

namespace Niffler.Rules
{
    class RulesManager : IResetState
    {
        public SellLimitOrdersTrader SellLimitOrdersTrader { get; }
        public BuyLimitOrdersTrader BuyLimitOrdersTrader { get; }
        public PositionsManager PositionsManager { get; }
        public SpikeManager SpikeManager { get; }
        public StopLossManager StopLossManager { get; }
        public FixedTrailingStop FixedTrailingStop { get; }
        public State BotState { get; }
        public GooglePubSubBroker MessageBroker {get;}
        private Reporter Reporter { get; set; }
        private List<IRule> OnTickRules = new List<IRule>();
        private List<IRuleOnPositionEvent> OnPositionOpenedRules = new List<IRuleOnPositionEvent>();
        private List<IRuleOnPositionEvent> OnPositionClosedRules = new List<IRuleOnPositionEvent>();
        private List<IRule> OnTimerRules = new List<IRule>();
        private List<IRule> OnBarRules = new List<IRule>();

        public RulesManager(State botState, SellLimitOrdersTrader sellLimitOrdersTrader, BuyLimitOrdersTrader buyLimitOrdersTrader, StopLossManager stopLossManager, FixedTrailingStop fixedTrailingStop)
        {
            BotState = botState;
            Reporter = botState.GetReporter();
            SellLimitOrdersTrader = sellLimitOrdersTrader;
            BuyLimitOrdersTrader = buyLimitOrdersTrader;
            PositionsManager = new PositionsManager(BotState);
            SpikeManager = new SpikeManager(BotState);
            StopLossManager = stopLossManager;
            FixedTrailingStop = fixedTrailingStop;

            try
            {
                MessageBroker = new GooglePubSubBroker();
            }
            catch (Exception e)
            {
                BotState.Bot.Print(e);
            }
        }

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

        public void SetOnPositionClosedRule(IRuleOnPositionEvent rule)
        {
            rule.Init(this);
            OnPositionClosedRules.Add(rule);
            OnPositionClosedRules = OnPositionClosedRules.OrderBy(r => r.Priority).ToList();
        }

        public void SetOnPositionOpenedRule(IRuleOnPositionEvent rule)
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

        public void SetOnPositionClosedRules(List<IRuleOnPositionEvent> rules)
        {
            InitialiseRules(rules.ConvertAll(x => (IRule)x));
            OnPositionClosedRules = rules.OrderBy(rule => rule.Priority).ToList();
        }

        public void SetOnPositionOpenedRules(List<IRuleOnPositionEvent> rules)
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

        public void RunAllPositionRules(List<IRuleOnPositionEvent> rules,Position position)
        {
            rules.ForEach(IRuleOnPositionEvent => IRuleOnPositionEvent.Run(position));
        }

        public void RunAllRules(List<IRule> rules)
        {
            rules.ForEach(IRule => IRule.Run());
        }

        public void ResetRules()
        {
            OnTickRules.ForEach(IRule => IRule.ResetRule());
            OnBarRules.ForEach(IRule => IRule.ResetRule());
            OnTimerRules.ForEach(IRule => IRule.ResetRule());
            OnPositionClosedRules.ForEach(IRule => IRule.ResetRule());
            OnPositionOpenedRules.ForEach(IRule => IRule.ResetRule());
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
            BotState.GetReporter().Report();
        }


    }
}
