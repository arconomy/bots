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
    class CloseTimeSetHardSLToLastPositionEntryWithBuffer : IRule
    {
        public CloseTimeSetHardSLToLastPositionEntryWithBuffer(int priority) : base(priority) { }

        //After CLose time set hard stop losses at last position entry price with Buffer
        override protected void Execute()
        {
           
            if (BotState.IsAfterCloseTime)
            {
                StopLossManager.SetSLWithBufferForAllPositions(BotState.LastPositionEntryPrice);
                ExecuteOnceOnly();
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
