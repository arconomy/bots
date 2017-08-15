using System;
using Google.Protobuf.Collections;

namespace Niffler.Rules
{
    class OpenTimeCapturePrice : IRule
    {
        public OpenTimeCapturePrice(int priority) : base(priority) { }

        //If rule should only execute when bot is trading return TRUE, default is FALSE
        protected override bool IsTradingRule()
        {
            return true;
        }

        //Get the Opening price for the trading period
        override protected bool Execute()
        {
            if (BotState.IsOpenTime)
            {
                BotState.OpenPrice = Bot.Symbol.Ask + Bot.Symbol.Spread / 2;
                BotState.OpenPriceCaptured = true;
                ExecuteOnceOnly();
                return true;
            }
            return false;
        }

        // reset any botstate variables to the state prior to executing rule
        override protected void Reset()
        {
            BotState.OpenPriceCaptured = false;
            BotState.OpenPrice = 0;
        }

        // report stats on rule execution 
        // e.g. execution rate, last position rule applied to, number of positions impacted by rule
        override public MapField<String, String> GetLastExecutionData()
        {
            return new MapField<string, string>{ { "OpenPrice", BotState.OpenPrice.ToString()} };
            
        }

        //create name of Rule Topic for Pub/Sub
        public override string GetPubSubTopicName()
        {
            return "OpenTimeCapturePrice";
        }
    }
}
