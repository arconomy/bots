using System;
using Google.Protobuf.Collections;
using Niffler.Strategy;
using Niffler.Common;
using Niffler.Messaging.RabbitMQ;
using System.Collections.Generic;

namespace Niffler.Rules
{
    class OnOpenTime : IRule
    {
        private TimeZoneInfo TimeZone;
        private TimeSpan OpenTime; //  Open time for Bot to place new trades (not necessarily same as the actual market open)

        public OnOpenTime(IDictionary<string, string> botConfig, RuleConfig ruleConfig) : base(botConfig, ruleConfig) { }

        public override void Init()
        {
            RuleConfig.Params.TryGetValue("OpenTime", out string openTime);
            Utils.ParseStringToTimeSpan(openTime, ref OpenTime);
        }

        //If rule should only execute when bot is trading return TRUE, default is FALSE
        protected override bool IsTradingRule()
        {
            return true;
        }

        public override void MessageReceived(MessageReceivedEventArgs e)
        {
            //Execute logic and publish back to msg bus

            //If message says deactivate
            Shutdown();
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

        protected override List<RoutingKey> GetRoutingKeys()
        {
            RoutingKey routingKey1 = new RoutingKey();
            routingKey1.SetEntity("OpenTimeCapturePrice");

            RoutingKey routingKey2 = new RoutingKey();
            routingKey2.SetEvent("OnTick");

            List<RoutingKey> routingKeys = new List<RoutingKey>
            {
                routingKey1,
                routingKey2
            };

            return routingKeys;
        }

        public override object Clone()
        {
            return new OnOpenTime(BotConfig, RuleConfig);
        }
    }
}
