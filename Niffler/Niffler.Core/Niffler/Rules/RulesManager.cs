using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Niffler.Rules
{
    class RulesManager
    {



        public void executeRule(Rule rule)
        {

        }


        public void executeAllRules()
        {

            //Close any positions that have not been triggered
            if (!BotState.IsPendingOrdersClosed)
                CloseAllPendingOrders();

            if (retraceEnabled)
            {
                //Calculate spike retrace factor
                double retraceFactor = calculateRetraceFactor();

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
