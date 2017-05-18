using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Niffler.Common;
using Niffler.Common.Trade;
using Niffler.Common.Market;

namespace Niffler.Rules
{
    class RulesManager
    {

        //TO DO = The RulesManager should be a 'Client' created in the context of the TradeStrategy which determines the Factory to created
        // The SwfRuleFactory will create the Rule Objects for each of the Rules it uses
        // The DiviFactory will create the Rules Objects for each of the Rules it uses
        // The Rules Objects can be created by either Factory

        public OrdersManager OrdersManager { get; }
        public PositionsManager PositionsManager { get; }
        public SpikeManager SpikeManager { get; }
        public StopLossManager StopLossManager { get; }
        public State BotState { get; }
        private List<IRule> Rules;

        public RulesManager(State botState, OrdersManager ordersManager, SpikeManager spikeManager, StopLossManager stopLossManager)
        {
            BotState = botState;
            OrdersManager = ordersManager;
            PositionsManager = new PositionsManager(BotState);
            SpikeManager = spikeManager;
            StopLossManager = stopLossManager;
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
                            new CloseAllPendingOrders(this)
                        };
                    }
                case State.BotType.DIVIDEND:
                    {
                        //Create Dividend Bot Rules
                        return new List<IRule>
                        {
                            new CloseAllPendingOrders(this)
                        };
                    }
                default:
                    return new List<IRule> { };
            }

        }

        public void executeRule(IRule rule)
        {

        }

        public void executeAllRules()
        {
            CloseAllPendingOrdersRule.execute();

            //Calculate spike retrace factor
            SpikeManager.calculateRetraceFactor();

            //Execute rulles that impact retrace levels
            ReduceRetraceLevelsAfterReduceRiskTime.execute();

            //execute All Retrace Rules
            ReduceRetraceLevelsAfterReduceRiskTime.execute();


                if (BotState.IsReducedRiskTime)
                {
                    //reset HARD SL Limits with reduced SL's
                    BotState.IsHardSLLastPositionEntryPrice = true;

                    //Reduce all retrace limits
                    RetraceLevel1 = RetraceLevel1 / 2;
                    RetraceLevel2 = RetraceLevel2 / 2;
                    RetraceLevel3 = RetraceLevel3 / 2;
                }

                //Set hard stop losses as soon as Swordfish time is over
                if (!BotState.IsHardSLLastPositionEntryPrice && !IsSwordFishTime())
                {
                    StopLossManager.setSLWithBufferForAllPositions(BotState.LastPositionEntryPrice);
                    BotState.IsHardSLLastPositionEntryPrice = true;
                }

                //Set hard stop losses and activate Trail if Spike has retraced between than retraceLevel1 and retraceLevel2
                if (BotState.IsReducedRiskTime || (RetraceLevel2 > retraceFactor && retraceFactor > RetraceLevel1))
                {
                    //If Hard SL has not been set yet
                    if (!BotState.IsHardSLLastPositionEntryPrice && BotState.LastPositionEntryPrice > 0)
                    {
                        StopLossManager.setSLWithBufferForAllPositions(BotState.LastPositionEntryPrice);
                        BotState.IsHardSLLastPositionEntryPrice = true;
                    }
                    //Active Breakeven Stop Losses
                    BotState.IsBreakEvenStopLossActive = true;
                }

                //Set harder SL and active BreakEven if it has retraced between than retraceLevel2 and retraceLevel3
                if (BotState.IsReducedRiskTime || (RetraceLevel3 > retraceFactor && retraceFactor > RetraceLevel2))
                {
                    //Set hard stop losses
                    if (!BotState.IsHardSLLastClosedPositionEntryPrice && BotState.LastClosedPositionEntryPrice > 0)
                    {
                        StopLossManager.setSLForAllPositions(BotState.LastClosedPositionEntryPrice);
                        BotState.IsHardSLLastClosedPositionEntryPrice = true;
                    }
                    //Activate Trailing Stop Losses
                    TrailingStop.IsActive = true;
                }

                //Set hardest SL if Spike retraced past retraceLevel3
                if (BotState.IsReducedRiskTime || retraceFactor > retraceLevel3)
                {
                    //Set hard stop losses
                    if (!BotState.IsHardSLLastProfitPrice && BotState.LastProfitPrice > 0)
                    {
                        setAllStopLosses(BotState.LastProfitPrice);
                        BotState.IsHardSLLastProfitPrice = true;
                    }
                }
            }
        }
    }
}
