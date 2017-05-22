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

        public RulesManager(State botState, SellLimitOrdersTrader sellLimitOrdersTrader, BuyLimitOrdersTrader buyLimitOrdersTrader, SpikeManager spikeManager, StopLossManager stopLossManager, FixedTrailingStop fixedTrailingStop)
        {
            BotState = botState;
            SellLimitOrdersTrader = sellLimitOrdersTrader;
            BuyLimitOrdersTrader = buyLimitOrdersTrader;
            PositionsManager = new PositionsManager(BotState);
            SpikeManager = spikeManager;
            StopLossManager = stopLossManager;
            FixedTrailingStop = fixedTrailingStop;
        }


        public void setOnTickRules(List<IRule> rules)
        {
            initialiseRules(rules);
            OnTickRules = rules.OrderBy(rule => rule.Priority).ToList();
        }

        public void setOnPositionClosedRules(List<IRuleOnPositionEvent> rules)
        {
            initialiseRules(rules.ConvertAll(x => (IRule)x));
            OnPositionClosedRules = rules.OrderBy(rule => rule.Priority).ToList();
        }

        public void setOnPositionOpenedRules(List<IRuleOnPositionEvent> rules)
        {
            initialiseRules(rules.ConvertAll(x => (IRule)x));
            OnPositionOpenedRules = rules.OrderBy(rule => rule.Priority).ToList();
        }

        private void initialiseRules(List<IRule> rules)
        {
            rules.ForEach(rule => rule.init(this));
        }

        public void onTick()
        {
            //Check the state based on the time from open
            BotState.checkTimeState();

            //Run the onTick Rules
            runAllRules(OnTickRules);
        }

        //Run the onBar Rules
        public void onBar()
        {
            runAllRules(OnBarRules);
        }

        public void onPositionClosed(Position position)
        {
            runAllPositionRules(OnPositionClosedRules, position);
        }

        public void onPositionOpened(Position position)
        {
            runAllPositionRules(OnPositionOpenedRules, position);
        }

        public void onTimer()
        {

        }

        public void executeRule(IRule rule)
        {
            rule.run();
        }

        public void runAllPositionRules(List<IRuleOnPositionEvent> rules,Position position)
        {
            rules.ForEach(IRuleOnPositionEvent => IRuleOnPositionEvent.run(position));
        }

        public void runAllRules(List<IRule> rules)
        {
            rules.ForEach(IRule => IRule.run());
        }

        public void reset()
        {
            SpikeManager.reset();
            FixedTrailingStop.reset();
            BotState.reset();
        }

    }
}
