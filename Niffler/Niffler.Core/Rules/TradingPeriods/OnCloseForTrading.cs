using System;
using Niffler.Messaging.RabbitMQ;
using System.Collections.Generic;
using Niffler.Messaging.Protobuf;
using Niffler.Services;
using Niffler.Core.Strategy;
using Niffler.Common;

namespace Niffler.Rules.TradingPeriods
{
    class OnCloseForTrading : IRule
    {
        private DateTimeZoneCalculator DateTimeZoneCalc;
        private string SymbolCode;
        private TimeSpan CloseTime;
        private TimeSpan CloseAfterOpen;

        public OnCloseForTrading(StrategyConfiguration StrategyConfig, RuleConfiguration ruleConfig) : base(StrategyConfig, ruleConfig) { }

        //Retreive data from config to initialise the rule
        public override void Init()
        {
            //At a minumum need SymbolCode & CloseTime or CloseMinsFromOpen
            SymbolCode = StrategyConfig.Config.Exchange;
            if (String.IsNullOrEmpty(SymbolCode)) IsInitialised = false;

            bool initSuccess = false;
            if (RuleConfig.Params.TryGetValue(RuleConfiguration.CLOSEAFTEROPEN, out object closeAfterOpen))
            {
                if (TimeSpan.TryParse(closeAfterOpen.ToString(), out CloseAfterOpen)) initSuccess = true;
            }
            
            if (RuleConfig.Params.TryGetValue(RuleConfiguration.CLOSETIME, out object closeTime))
            {
                if (TimeSpan.TryParse(closeTime.ToString(), out CloseTime)) initSuccess = true;
            }

            if (!initSuccess) IsInitialised = false;
            
            DateTimeZoneCalc = new DateTimeZoneCalculator(SymbolCode);

            //Wait until OpenForTrading notifies before becoming active
            IsActive = false; 
        }
        
        //Execute rule logic
        override protected bool ExcuteRuleLogic(Niffle message)
        {
            if (IsTickMessageEmpty(message)) return false;

            DateTime Now = DateTime.FromBinary(message.Tick.TimeStamp);

            //First Tick recieved after service is activated will be the first tick after OpenForTrading has sent notified this service - therefore use this Tick time as OpenTime
            if (CloseAfterOpen > TimeSpan.Zero)
            {
                SetCloseTime(Now);
            }

            if (DateTimeZoneCalc.IsTimeAfter(Now, CloseTime))
            {
                PublishStateUpdate(Data.State.ISOPENTIME, false);
                IsActive = false;
                return true;
            }
            return false;
        }

        private void SetCloseTime(DateTime openTime)
        {
            if (CloseAfterOpen != TimeSpan.Zero)
            {
                CloseTime = openTime.TimeOfDay;
                CloseTime.Add(CloseAfterOpen);

                //Set CloseAfterOpen to TimeSpan.Zero so that CloseTime is only updated once.
                CloseAfterOpen = TimeSpan.Zero;
            }
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
                if(message.State.Key == Data.State.OPENTIME && message.State.ValueType == Messaging.Protobuf.State.Types.ValueType.String)
                {
                    DateTime.TryParse(message.State.StringValue, out DateTime opentime);
                    SetCloseTime(opentime);
                }
            }
        }

        protected override string GetServiceName()
        {
            return nameof(OnCloseForTrading);
        }

        protected override List<RoutingKey> SetListeningRoutingKeys()
        {
            //Listen for OnTick
            List < RoutingKey > routingKeys = RoutingKey.Create(Source.WILDCARD, Messaging.RabbitMQ.Action.WILDCARD, Event.ONTICK).ToList();

            //Listen for successful Service execution Notification from OnOpenForTrading
            routingKeys.Add(RoutingKey.Create(nameof(OnOpenForTrading), Messaging.RabbitMQ.Action.NOTIFY, Event.WILDCARD));

            //Listen for Update State Notification from StateManager for the Open Time
            routingKeys.Add(RoutingKey.Create(nameof(StateManager), Messaging.RabbitMQ.Action.UPDATESTATE, Event.WILDCARD));
            return routingKeys;
        }

        public override void Reset()
        {
            //Wait until OpenForTrading notifies before becoming active
            IsActive = false;
        }
    }
}
