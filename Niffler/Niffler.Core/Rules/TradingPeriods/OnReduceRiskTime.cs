using System;
using Niffler.Messaging.Protobuf;
using Niffler.Messaging.RabbitMQ;
using System.Collections.Generic;
using Niffler.Rules.TradingPeriods;
using Niffler.Services;
using Niffler.Core.Config;
using Niffler.Common;

namespace Niffler.Rules.TradingPeriods
{
    class OnReduceRiskTime : IRule
    {
        private string SymbolCode;
        private TimeSpan ReduceRiskTime; // ReduceRisk time for Strategy to manage trades    
        private TimeSpan ReduceRiskAfterOpen; // TimeSpan after OpenForTradingTime to ReduceRisk
        private DateTimeZoneCalculator DateTimeZoneCalc;

        public OnReduceRiskTime(StrategyConfiguration strategyConfig, RuleConfiguration ruleConfig) : base(strategyConfig, ruleConfig) { }

        //Retreive data from config to initialise the rule
        public override void Init()
        {
            //At a minumum need SymbolCode & CloseTime or CloseMinsFromOpen
            SymbolCode = StrategyConfig.Exchange;
            if (SymbolCode == "" || SymbolCode == null) IsInitialised = false;

            bool initSuccess = false;
            if (RuleConfig.Params.TryGetValue(RuleConfiguration.REDUCERISKAFTEROPEN, out object reduceRiskAfterOpen))
            {
                if (TimeSpan.TryParse(reduceRiskAfterOpen.ToString(), out ReduceRiskAfterOpen)) initSuccess = true;
            }

            if (RuleConfig.Params.TryGetValue(RuleConfiguration.REDUCERISKTIME, out object reduceRiskTime))
            {
                if (TimeSpan.TryParse(reduceRiskTime.ToString(), out ReduceRiskTime)) initSuccess = true;
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
            if (ReduceRiskTime == TimeSpan.Zero) return false;

            DateTime Now = DateTime.FromBinary(message.Tick.TimeStamp);

            if (DateTimeZoneCalc.IsTimeAfter(Now, ReduceRiskTime))
            {
                PublishStateUpdate(Data.State.ISREDUCERISKTIME, true);
                PublishStateUpdate(Data.State.REDUCERISKTIME, message.Tick.TimeStamp);
                IsActive = false;
                return true;
            }
            return false;
        }

        private void SetReduceRiskTime(DateTime openTime)
        {
            if (ReduceRiskAfterOpen != TimeSpan.Zero)
            {
                ReduceRiskTime = openTime.TimeOfDay;
                ReduceRiskTime = ReduceRiskTime.Add(ReduceRiskAfterOpen);

                //Set ReduceRiskAfterOpen to TimeSpan.Zero so that ReduceRisk is only updated once.
                ReduceRiskAfterOpen = TimeSpan.Zero;
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
            //Listen to updateState msg from OpenTrading Service
            if (routingKey.Source == nameof(OnOpenForTrading))
            {
                if (message.State.Key == Data.State.OPENTIME && message.State.ValueType == Messaging.Protobuf.State.Types.ValueType.Datetimelong)
                {
                    SetReduceRiskTime(DateTime.FromBinary(message.State.LongValue));
                }
            }
        }

        protected override string GetServiceName()
        {
            return nameof(OnReduceRiskTime);
        }

        protected override List<RoutingKey> SetListeningRoutingKeys()
        {
            //Listen for OnTick
            List<RoutingKey> routingKeys = RoutingKey.Create(Source.WILDCARD, Messaging.RabbitMQ.Action.WILDCARD, Event.ONTICK).ToList();

            //Listen for successful Service execution Notification from OnOpenForTrading
            routingKeys.Add(RoutingKey.Create(nameof(OnOpenForTrading), Messaging.RabbitMQ.Action.NOTIFY, Event.WILDCARD));

            //Listen for Update State Notification from OnOpenForTrading for the Open Time
            routingKeys.Add(RoutingKey.Create(nameof(OnOpenForTrading), Messaging.RabbitMQ.Action.UPDATESTATE, Event.WILDCARD));

            return routingKeys;
        }

        public override void Reset()
        {
            //Wait until OpenForTrading notifies before becoming active
            IsActive = false;
        }
    }
}
