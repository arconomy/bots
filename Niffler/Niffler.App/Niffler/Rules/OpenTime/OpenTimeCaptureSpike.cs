using System;
using Google.Protobuf.Collections;

namespace Niffler.Rules
{
    class OpenTimeCaptureSpike : IRule
    {
        public OpenTimeCaptureSpike(int priority) : base(priority) {}

        //If rule should only execute when bot is trading return TRUE, default is FALSE
        protected override bool IsTradingRule()
        {
            return false;
        }

        // If Trading time then capture spike
        override protected bool ExcuteRuleLogic()
        {
            if (MarketInfo.IsBotTradingOpen())
            {
                SpikeManager.CaptureSpike();
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
