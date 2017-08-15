using System;
using Google.Protobuf.Collections;

namespace Niffler.Rules
{
    class TerminateTimeCloseAllPositionsReset : IRule
    {
        public TerminateTimeCloseAllPositionsReset(int priority) : base(priority) { }

        //If rule should only execute when bot is trading return TRUE, default is FALSE
        protected override bool IsTradingRule()
        {
            return true;
        }

        //If trades still open at Terminate Time then take the hit and close remaining positions
        override protected bool Execute()
        {
            if(BotState.IsAfterTerminateTime)
            {
                PositionsManager.CloseAllPositions();
                BotState.IsReset = true;
                return true;
            }
            return false;
        }

        // reset any botstate variables to the state prior to executing rule
        override protected void Reset()
        {
            BotState.IsReset = false;
        }

        // report stats on rule execution 
        // e.g. execution rate, last position rule applied to, number of positions impacted by rule
        override public MapField<String, String> GetLastExecutionData()
        {
            return new MapField<string, string> { { "result", "success" } };

        }

        //create name of Rule Topic for Pub/Sub
        public override string GetPubSubTopicName()
        {
            return this.GetType().Name;
        }
    }
}
