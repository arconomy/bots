using System;
using Google.Protobuf.Collections;

namespace Niffler.Rules
{
    class TerminateTimeSetBotState : IRule
    {
        public TerminateTimeSetBotState(int priority) : base(priority) { }

        //If rule should only execute when bot is trading return TRUE, default is FALSE
        protected override bool IsTradingRule()
        {
            return true; //Terminate time is only interesting if the bot is trading
        }

        //After CLose time set hard stop losses at last position entry price with Buffer
        override protected bool ExcuteRuleLogic()
        {
            if (!BotState.IsAfterTerminateTime && MarketInfo.IsAfterTerminateTime())
            {
                BotState.IsAfterTerminateTime = true;
                return true;
            }
            return false;
        }

        // reset any botstate variables to the state prior to executing rule
        override protected void Reset()
        {
            BotState.IsAfterTerminateTime = false;
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
