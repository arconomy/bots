using System;
using Google.Protobuf.Collections;
using Niffler.Strategy;
using Niffler.Common;

namespace Niffler.Rules
{
    class OnCloseTime : IRule
    {
        public OnCloseTime() : base() { }

        private TimeZoneInfo TimeZone;
        private TimeSpan OpenTime; //  Open time for Bot to place new trades (not necessarily same as the actual market open)
        private TimeSpan CloseTime; // Close time for Bot to place new trades (not necessarily same as the actual market close)
        private TimeSpan CloseAfter; // TimeSpan after OpenTime to Closed for Bot to place new trades
        private bool UseCloseTime;


        public override void Init(RuleConfig ruleConfig)
        {
            ruleConfig.Params.TryGetValue("OpenTime", out string openTime);
            Utils.ParseStringToTimeSpan(openTime, ref OpenTime);

            ruleConfig.Params.TryGetValue("CloseTime", out string closeTime);
            UseCloseTime = Utils.ParseStringToTimeSpan(closeTime, ref CloseTime);

            ruleConfig.Params.TryGetValue("CloseAfter", out string closeAfter);
            Utils.ParseStringToTimeSpan(closeAfter, ref CloseAfter);
        }

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
