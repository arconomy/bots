using System;
using Niffler.Messaging.Protobuf;
using Niffler.Messaging.RabbitMQ;
using System.Collections.Generic;
using Niffler.Rules.TradingPeriods;
using Niffler.Services;
using Niffler.Core.Strategy;
using Niffler.Common;

namespace Niffler.Rules.TradingPeriods
{
    class OnTerminateTime : IRule
    {
        private string SymbolCode;
        private TimeSpan TerminateTime; // Terminate Bot activity after this time
        private TimeSpan TerminateAfterOpen; // TimeSpan after OpenTime to ReduceRisk Terminate Bot activity
        private DateTimeZoneCalculator DateTimeZoneCalc;

        public OnTerminateTime(StrategyConfiguration strategyConfig, RuleConfiguration ruleConfig) : base(strategyConfig, ruleConfig) { }

        //Retreive data from config to initialise the rule
        public override void Init()
        {
            //At a minumum need SymbolCode & CloseTime or CloseMinsFromOpen
            SymbolCode = StrategyConfig.Config.Exchange;
            if (String.IsNullOrEmpty(SymbolCode)) IsInitialised = false;

            bool initSuccess = false;
            if (RuleConfig.Params.TryGetValue(RuleConfiguration.TERMINATEAFTEROPEN, out object terminateAfterOpen))
            {
                if (TimeSpan.TryParse(terminateAfterOpen.ToString(), out TerminateAfterOpen)) initSuccess = true;
            }

            if (RuleConfig.Params.TryGetValue(RuleConfiguration.TERMINATETIME, out object terminateTime))
            {
                if (TimeSpan.TryParse(terminateTime.ToString(), out TerminateTime)) initSuccess = true;
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

            //First Tick recieved will be the first after OpenForTrading has service has notified this service - therefore use this Tick time as OpenTime
            if (TerminateAfterOpen > TimeSpan.Zero)
            {
                SetTerminateTime(Now);
            }

            if (DateTimeZoneCalc.IsTimeAfter(Now, TerminateTime))
            {
                PublishStateUpdate(Data.State.ISTERMINATETIME, true);
                IsActive = false;
                return true;
            }
            return false;
        }

        private void SetTerminateTime(DateTime openTime)
        {
            if(TerminateAfterOpen != TimeSpan.Zero)
            {
                TerminateTime = openTime.TimeOfDay;
                TerminateTime.Add(TerminateAfterOpen);

                //Set CloseAfterOpen to TimeSpan.Zero so that CloseTime is only updated once.
                TerminateAfterOpen = TimeSpan.Zero;
            }
        }

        override protected void OnServiceNotify(Niffle message, RoutingKey routingKey)
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
                if (message.State.Key == Data.State.OPENTIME && message.State.ValueType == Messaging.Protobuf.State.Types.ValueType.String)
                {
                    DateTime.TryParse(message.State.StringValue, out DateTime opentime);
                    SetTerminateTime(opentime);
                }
            }
        }

        protected override string GetServiceName()
        {
            return nameof(OnTerminateTime);
        }

        protected override List<RoutingKey> SetListeningRoutingKeys()
        {
            //Listen for OnTick
            List<RoutingKey> routingKeys = RoutingKey.Create(Source.WILDCARD, Messaging.RabbitMQ.Action.WILDCARD, Event.ONTICK).ToList();

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

