using System;
using Google.Protobuf.Collections;

namespace Niffler.Rules
{
    class CloseTimeSetBotState : IRule
    {
        public CloseTimeSetBotState(int priority) : base(priority) { }

        //If rule should only execute when bot is trading return TRUE, default is FALSE
        protected override bool IsTradingRule()
        {
            return true;  //Close time is only interesting if the bot is trading
        }

        //After CLose time set hard stop losses at last position entry price with Buffer
        override protected bool Execute()
        {
            if (!BotState.IsAfterCloseTime && MarketInfo.IsAfterCloseTime())
            {
                BotState.IsAfterCloseTime = true;
                ExecuteOnceOnly();
                return true;
            }
            return false;
        }

        override protected void Reset()
        {
            BotState.IsAfterCloseTime = false;
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
