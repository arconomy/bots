using System;
using Google.Protobuf.Collections;
using Niffler.Strategy;
using Niffler.Common;
using Niffler.Messaging.Protobuf;
using Niffler.Messaging.RabbitMQ;
using System.Collections.Generic;

namespace Niffler.Rules
{
    class OnTerminateTime : IRule
    {
        public OnTerminateTime() : base() { }

        private TimeZoneInfo TimeZone;
        private TimeSpan OpenTime; //  Open time for Bot to place new trades (not necessarily same as the actual market open)
        private TimeSpan TerminateTime; // Terminate Bot activity after this time
        private TimeSpan TerminateAfter; // TimeSpan after OpenTime to ReduceRisk Terminate Bot activity
        private bool UseTerminateTime;

        public override void Init(RuleConfiguration ruleConfig)
        {
            ruleConfig.Params.TryGetValue("OpenTime", out string openTime);
            Utils.ParseStringToTimeSpan(openTime, ref OpenTime);

            ruleConfig.Params.TryGetValue("TerminateTime", out string terminateTime);
            UseTerminateTime = Utils.ParseStringToTimeSpan(terminateTime, ref TerminateTime);

            ruleConfig.Params.TryGetValue("TerminateAfter", out string terminateAfter);
            Utils.ParseStringToTimeSpan(terminateAfter, ref TerminateAfter);

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

        protected override string GetServiceName()
        {
            throw new NotImplementedException();
        }

        protected override bool ExcuteRuleLogic(Niffle message)
        {
            throw new NotImplementedException();
        }

        protected override List<RoutingKey> SetListeningRoutingKeys()
        {
            throw new NotImplementedException();
        }

        protected override void OnServiceNotify(Niffle message, RoutingKey routingKey)
        {
            throw new NotImplementedException();
        }

        protected override void OnStateUpdate(Niffle message, RoutingKey routingKey)
        {
            throw new NotImplementedException();
        }

        public override object Clone()
        {
            throw new NotImplementedException();
        }

        public override bool Init()
        {
            throw new NotImplementedException();
        }
    }
}
