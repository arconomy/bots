using System;
using System.Collections.Generic;
using Google.Protobuf.Collections;
using Niffler.Messaging.Protobuf;
using Niffler.Messaging.RabbitMQ;
using Niffler.Strategy;

namespace Niffler.Rules
{
    class OpenTimeCapturePrice : IRule
    {
        public OpenTimeCapturePrice() : base() { }


        public override void Init(RuleConfiguration ruleConfig)
        {

            



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
