using System;
using Google.Protobuf.Collections;
using Niffler.Strategy;
using Niffler.Common;

namespace Niffler.Rules
{
    class OnReduceRiskTime : IRule
    {
        public OnReduceRiskTime() : base() { }

        private TimeZoneInfo TimeZone;
        private TimeSpan OpenTime; //  Open time for Bot to place new trades (not necessarily same as the actual market open)
        private TimeSpan ReduceRiskTime; // ReduceRisk time for Bot to manage trades (not necessarily same as the actual market close)    
        private TimeSpan ReduceRiskAfter; // TimeSpan after OpenTime to ReduceRisk
        private bool UseReduceRiskTime;

        public override void Init(RuleConfig ruleConfig)
        {
            ruleConfig.Params.TryGetValue("OpenTime", out string openTime);
            Utils.ParseStringToTimeSpan(openTime, ref OpenTime);

            ruleConfig.Params.TryGetValue("ReduceRiskTime", out string reduceRiskTime);
            UseReduceRiskTime = Utils.ParseStringToTimeSpan(reduceRiskTime, ref ReduceRiskTime);

            ruleConfig.Params.TryGetValue("ReduceRiskAfter", out string reduceRiskAfter);
            Utils.ParseStringToTimeSpan(reduceRiskAfter, ref ReduceRiskAfter);
        }

        //If rule should only execute when bot is trading return TRUE, default is FALSE
        protected override bool IsTradingRule()
        {
            return true;
        }

        //Get the Opening price for the trading period
        override protected bool ExcuteRuleLogic()
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
