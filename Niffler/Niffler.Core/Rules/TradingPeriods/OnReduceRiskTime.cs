using System;
using Niffler.Messaging.Protobuf;
using Niffler.Messaging.RabbitMQ;
using System.Collections.Generic;
using Niffler.Rules.TradingPeriods;
using Niffler.Services;
using Niffler.Core.Config;
using Niffler.Common;
using Niffler.Model;

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

            //Listen for state updates for the OpenTime
            StateManager.ListenForStateUpdates();
        }


        //Execute rule logic
        override protected bool ExcuteRuleLogic(Niffle message)
        {
            if (IsTickMessageEmpty(message)) return false;
            if (ReduceRiskTime == TimeSpan.Zero) return false;

            DateTime Now = DateTime.FromBinary(message.Tick.TimeStamp);

            if (DateTimeZoneCalc.IsTimeAfter(Now, ReduceRiskTime))
            {

                StateManager.SetInitialState(new State
                    {
                        { State.ISREDUCERISKTIME, true },
                        { State.REDUCERISKTIME, message.Tick.TimeStamp }
                    });

                SetActiveState(false);
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
            //Nothing required
        }

        protected override void OnStateUpdate(StateChangedEventArgs stateupdate)
        {
            //Listening for update to OpenTime State
            if (stateupdate.Key == Model.State.OPENTIME)
            {
                if (long.TryParse(stateupdate.Value.ToString(), out long openTime))
                    SetReduceRiskTime(DateTime.FromBinary(openTime));
            }
        }

        protected override string GetServiceName()
        {
            return nameof(OnReduceRiskTime);
        }

        protected override void AddListeningRoutingKeys(ref List<RoutingKey> routingKeys)
        {
            //Listen for OnTick
            routingKeys.Add(RoutingKey.Create(Source.WILDCARD, Messaging.RabbitMQ.Action.WILDCARD, Event.ONTICK));
        }

        public override void Reset()
        {
            //Wait until OpenForTrading notifies before becoming active
            SetActiveState(false);
        }
    }
}
