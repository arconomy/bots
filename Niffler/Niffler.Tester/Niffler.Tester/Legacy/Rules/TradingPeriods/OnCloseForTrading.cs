using System;
using Google.Protobuf.Collections;
using Niffler.Strategy;
using Niffler.Common;
using Niffler.Messaging.RabbitMQ;
using System.Collections.Generic;
using Niffler.Common.Helpers;
using System.Collections;
using Niffler.Messaging.Protobuf;
using Niffler.Microservices;
using Niffler.Managers;

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

        public OnCloseForTrading(IDictionary<string, string> botConfig, RuleConfiguration ruleConfig) : base(botConfig, ruleConfig) { }

        //Retreive data from config to initialise the rule
        public override bool Init()
        {
            //At a minumum need SymbolCode & CloseTime or CloseMinsFromOpen
            if(StrategyConfig.TryGetValue(StrategyConfiguration.MARKET, out SymbolCode)) return false;
            if (SymbolCode == "" || SymbolCode == null) return false;

            bool initSuccess = false;
            if (RuleConfig.Params.TryGetValue(RuleConfiguration.CLOSEAFTEROPEN, out object closeAfterOpen))
            {
                if (TimeSpan.TryParse(closeAfterOpen.ToString(), out CloseAfterOpen)) initSuccess = true;
            }
            
            if (RuleConfig.Params.TryGetValue(RuleConfiguration.CLOSETIME, out object closeTime))
            {
                if (TimeSpan.TryParse(closeTime.ToString(), out CloseTime)) initSuccess = true;
            }
            
            DateTimeZoneCalc = new DateTimeZoneCalculator(SymbolCode);

            //Wait until OpenForTrading notifies before becoming active
            IsActive = false; 
            return initSuccess;
        }
        
        //Execute rule logic
        override protected bool ExcuteRuleLogic(Niffle message)
        {
            if (IsTickMessageEmpty(message)) return false;

            if(DateTime.TryParse(message.Tick.Timestamp, out Now))
            {
                //First Tick recieved will be the first after OpenForTrading has service has notified this service - therefore use this Tick time as OpenTime
                //The OpenTime may be updated by receiving an update to state data
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

            //Listening for OpenForTrading notification to activate
            if (routingKey.Source == nameof(OnOpenForTrading) && message.Service.Success)
            {
                IsActive = true;
            }
        }

        protected override void OnStateUpdate(Niffle message, RoutingKey routingKey)
        {
            if (IsStateMessageEmpty(message)) return;

            //Could listen to updateState msg from OpenTrading Service, but better to get state updates from the State Manager
            if (routingKey.Source == nameof(StateManager))
            {
                if(message.State.Key == StateData.OPENTIME && message.State.Valuetype == State.Types.ValueType.String)
                {
                    DateTime.TryParse(message.State.Stringvalue, out DateTime opentime);
                    SetCloseTime(opentime);
                }
            }
        }

        public override object Clone()
        {
            return new OnCloseForTrading(StrategyConfig, RuleConfig);
        }

        protected override string GetServiceName()
        {
            return nameof(OnCloseForTrading);
        }

        protected override List<RoutingKey> SetListeningRoutingKeys()
        {
            //Listen for OnTick
            List < RoutingKey > routingKeys = RoutingKey.Create(Source.WILDCARD, Messaging.RabbitMQ.Action.WILDCARD, Event.ONTICK).ToList();

            //Listen for Update State Notification from OnOpenForTrading
            routingKeys.Add(RoutingKey.Create(nameof(OnOpenForTrading), Messaging.RabbitMQ.Action.UPDATESTATE, Event.WILDCARD));

            //Listen for Update State Notification from StateManager for the Open Time
            routingKeys.Add(RoutingKey.Create(nameof(OnOpenForTrading), Messaging.RabbitMQ.Action.UPDATESTATE, Event.WILDCARD));
            return routingKeys;
        }
    }
}
