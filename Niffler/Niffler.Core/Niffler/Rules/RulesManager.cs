using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Niffler.Common;
using Niffler.Common.Trade;
using Niffler.Common.Market;
using Niffler.Common.TrailingStop;

namespace Niffler.Rules
{
    class RulesManager : IResetState
    {

        //TO DO = The RulesManager should be a 'Client' created in the context of the TradeStrategy which determines the Factory to created
        // The SwfRuleFactory will create the Rule Objects for each of the Rules it uses
        // The DiviFactory will create the Rules Objects for each of the Rules it uses
        // The Rules Objects can be created by either Factory

        public OrdersManager OrdersManager { get; }
        public PositionsManager PositionsManager { get; }
        public SpikeManager SpikeManager { get; }
        public StopLossManager StopLossManager { get; }
        public FixedTrailingStop FixedTrailingStop { get;}
        public State BotState { get; }
        private List<IRule> Rules;

        public RulesManager(State botState, OrdersManager ordersManager, SpikeManager spikeManager, StopLossManager stopLossManager, FixedTrailingStop fixedTrailingStop)
        {
            BotState = botState;
            OrdersManager = ordersManager;
            PositionsManager = new PositionsManager(BotState);
            SpikeManager = spikeManager;
            StopLossManager = stopLossManager;
            FixedTrailingStop = fixedTrailingStop;
            Rules = getRules();
        }

        private List<IRule> getRules()
        {
            switch (BotState.Type)
            {
                case State.BotType.SWORDFISH:
                    {
                        //Create Swordfish Bot Rules
                        return new List<IRule>
                        {
                            new CloseTimeCancelPendingOrders(this,1),
                            new CloseTimeSetHardSLToLastPositionEntryWithBuffer(this,2),
                            new CloseTimeNoPositionsOpenReset(this,3),
                            new ReduceRiskTimeReduceRetraceLevels(this,3),
                            new ReduceRiskTimeSetHardSLToLastProfitPositionCloseWithBuffer(this,4),
                            new ReduceRiskTimeSetTrailingStop(this,5),
                            new RetracedLevel1To2SetHardSLToLastProfitPositionEntryWithBuffer(this,6),
                            new RetracedLevel1To2SetBreakEvenSLActive(this,7),
                            new RetracedLevel2To3SetHardSLToLastProfitPositionEntry(this,8),
                            new RetracedLevel2To3SetHardSLToLastProfitPositionEntry(this, 9),
                            new RetracedLevel3PlusReduceHardSLBuffer(this,10),
                            new RetracedLevel3PlusSetHardSLToLastProfitPositionCloseWithBuffer(this,11),
                            new SetBreakEvenSLPastLastProfitPositionEntry(this,12),
                            new SetFixedTrailingStop(this,13),
                            new TerminateTimeCloseAllPositionsReset(this,14)
                        }.OrderBy(rule => rule.Priority).ToList();
                    }
                case State.BotType.DIVIDEND:
                    {
                        //Create Dividend Bot Rules
                        return new List<IRule>
                        {
                            new CloseTimeCancelPendingOrders(this,1)
                        };
                    }
                default:
                    return new List<IRule> { };
            }

        }

        public void executeRule(IRule rule)
        {
            rule.run();
        }

        public void runAllRules()
        {
            Rules.ForEach(IRule => IRule.run());
        }

        public void reset()
        {
            SpikeManager.reset();
            FixedTrailingStop.reset();
            BotState.reset();

            BotState.IsReset();
        }

    }
}
