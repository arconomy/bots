using System;
using Google.Protobuf.Collections;

namespace Niffler.Rules
{
    class StartTrading : IRule
    {
        public StartTrading(int priority) : base(priority) { }

        //If rule should only execute when bot is trading return TRUE, default is FALSE
        protected override bool IsTradingRule()
        {
            return false;
        }

        //The IsTrading flag is the only state variable that is not reset by the rule that sets it
        override protected bool ExcuteRuleLogic()
        {
            if (BotState.IsOpenTime)
            {
                BotState.IsTrading = true;
                ExecuteOnceOnly();
                return true;
            }
            return false;
        }

        // reset any botstate variables to the state prior to executing rule
        override protected void Reset()
        {

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
