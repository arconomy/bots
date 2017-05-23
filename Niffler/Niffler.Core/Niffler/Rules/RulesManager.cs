using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Niffler.Common;
using Niffler.Common.Trade;
using Niffler.Common.Market;
using Niffler.Common.TrailingStop;
using cAlgo.API;

namespace Niffler.Rules
{
    class RulesManager : IResetState
    {

        //TO DO = The RulesManager should be a 'Client' created in the context of the TradeStrategy which determines the Factory to created
        // The SwfRuleFactory will create the Rule Objects for each of the Rules it uses
        // The DiviFactory will create the Rules Objects for each of the Rules it uses
        // The Rules Objects can be created by either Factory

        public SellLimitOrdersTrader SellLimitOrdersTrader { get; }
        public BuyLimitOrdersTrader BuyLimitOrdersTrader { get; }
        public PositionsManager PositionsManager { get; }
        public SpikeManager SpikeManager { get; }
        public StopLossManager StopLossManager { get; }
        public FixedTrailingStop FixedTrailingStop { get;}
        public State BotState { get; }
        private List<IRule> OnTickRules;
        private List<IRuleOnPositionEvent> OnPositionOpenedRules;
        private List<IRuleOnPositionEvent> OnPositionClosedRules;
        private List<IRule> OnTimerRules;
        private List<IRule> OnBarRules;

        public RulesManager(State botState, SellLimitOrdersTrader sellLimitOrdersTrader, BuyLimitOrdersTrader buyLimitOrdersTrader, StopLossManager stopLossManager, FixedTrailingStop fixedTrailingStop)
        {
            BotState = botState;
            SellLimitOrdersTrader = sellLimitOrdersTrader;
            BuyLimitOrdersTrader = buyLimitOrdersTrader;
            PositionsManager = new PositionsManager(BotState);
            SpikeManager = new SpikeManager(BotState);
            StopLossManager = stopLossManager;
            FixedTrailingStop = fixedTrailingStop;
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
            OnTimerRules = rules.OrderBy(rule => rule.Priority).ToList();
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
            //Check the state based on the time from open
            BotState.CheckTimeState();

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
            rules.ForEach(IRuleOnPositionEvent => IRuleOnPositionEvent.run(position));
        }

        public void RunAllRules(List<IRule> rules)
        {
            rules.ForEach(IRule => IRule.Run());
        }

        public void Reset()
        {
            SpikeManager.Reset();
            FixedTrailingStop.Reset();
            BotState.Reset();
        }

    }
}
