using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using cAlgo.API;
using cAlgo.API.Indicators;
using cAlgo.API.Internals;
using Niffler.Common;

namespace Niffler.Rules
{
    class RetracedLevel1To2SetHardSLToLastProfitPositionEntryWithBuffer : IRule
    {
        public RetracedLevel1To2SetHardSLToLastProfitPositionEntryWithBuffer(int priority) : base(priority) { }

        //If Spike retrace is greater than Level 1 but less than Level 2 set SL to last profit position entry price plus buffer
        override protected void Execute()
        {
            if (BotState.OrdersPlaced && BotState.PositionsRemainOpen())
            {
                //Calculate spike retrace factor
                SpikeManager.CalculateRetraceFactor();

                //Set hard stop losses and activate Trail if Spike has retraced between than retraceLevel1 and retraceLevel2
                if (SpikeManager.IsRetraceBetweenLevel1AndLevel2())
                {
                    //If Hard SL has not been set yet
                    if (BotState.LastProfitPositionEntryPrice > 0)
                    {
                        StopLossManager.SetSLWithBufferForAllPositions(BotState.LastProfitPositionEntryPrice);
                    }
                }
            }
        }

        override public void ReportExecution()
        {
            // report stats on rule execution 
            // e.g. execution rate, last position rule applied to, number of positions impacted by rule
            // Gonna need some thought here.
        }
    }
}
