using System;
using Google.Protobuf.Collections;
using Niffler.Strategy;
using Niffler.Common;
using Niffler.Messaging.RabbitMQ;
using System.Collections.Generic;
using Niffler.Common.Helpers;
using System.Collections;
using Niffler.Messaging.Protobuf;

namespace Niffler.Rules.TradingPeriods
{
    class OnCloseForTrading : IRule
    {
        private TimeZoneInfo TimeZone;
        private DateTimeZoneCalculator DateTimeZoneCalc;
        private string SymbolCode;
        private TimeSpan CloseTime;
        private TimeSpan CloseAfterOpen;
        DateTime Now;

        public OnCloseForTrading(IDictionary<string, string> botConfig, RuleConfig ruleConfig) : base(botConfig, ruleConfig) { }

        public override bool Init()
        {
            //At a minumum need SymbolCode & CloseTime or CloseMinsFromOpen
            if(BotConfig.TryGetValue("Market", out SymbolCode)) return false;
            if (SymbolCode == "" || SymbolCode == null) return false;

            bool initSuccess = false;
            if (RuleConfig.Params.TryGetValue("CloseAfterOpen", out object closeAfterOpen))
            {
                if (TimeSpan.TryParse(closeAfterOpen.ToString(), out CloseAfterOpen)) initSuccess = true;
            }
            
            if (RuleConfig.Params.TryGetValue("CloseTime", out object closeTime))
            {
                if (TimeSpan.TryParse(closeTime.ToString(), out CloseTime)) initSuccess = true;
            }
            
            DateTimeZoneCalc = new DateTimeZoneCalculator(SymbolCode);
            IsActive = false; //Wait until OpenForTrading notifies before becoming active
            return initSuccess;
        }
        
        //Get the Opening price for the trading period
        override protected bool ExcuteRuleLogic(Niffle message)
        {
            if (IsTickMessageEmpty(message)) return false;


            if(DateTime.TryParse(message.Tick.Timestamp, out Now))
            {
                //First Tick recieved will be the first after OpenForTrading has service has notified this service - therefore use this Tick time as OpenTime
                if (CloseAfterOpen > TimeSpan.Zero)
                {
                    SetCloseTime(Now);
                }

                if (DateTimeZoneCalc.IsTimeAfter(Now, CloseTime))
                {
                    IsActive = false;
                    return true;
                }
            }
            return false;
        }

        private void SetCloseTime(DateTime now)
        {
            CloseTime = now.TimeOfDay;
            CloseTime.Add(CloseAfterOpen);

            //Set CloseAfterOpen to TimeSpan.Zero so that CloseTime is only updated once.
            CloseAfterOpen = TimeSpan.Zero;
        }


        override protected void OnServiceNotify(Niffle message,RoutingKey routingKey)
        {
            if (IsServiceMessageEmpty(message)) return;

            //Listening for OpenForTrading
            if (routingKey.Entity == nameof(OnOpenForTrading) && message.Service.Success)
            {
                IsActive = true;
            }
        }

        protected override List<RoutingKey> GetListeningRoutingKeys()
        {
            //Listen for OnTick
            List<RoutingKey> routingKeys = RoutingKey.Create(Entity.WILDCARD, Messaging.RabbitMQ.Action.WILDCARD, Event.ONTICK).getRoutingKeyAsList();

            //Listen for Notification from OnOpenForTrading
            routingKeys.Add(RoutingKey.Create(nameof(OnOpenForTrading), Messaging.RabbitMQ.Action.NOTIFY, Event.WILDCARD));
            return routingKeys;
        }

        public override object Clone()
        {
            return new OnCloseForTrading(BotConfig, RuleConfig);
        }

        protected override string GetServiceName()
        {
            return nameof(OnCloseForTrading);
        }
    }
}
