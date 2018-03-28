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
            SymbolCode = StrategyConfig.Exchange;
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

            //Listen for state updates for the OpenTime
            StateManager.ListenForStateUpdates();
        }


        //Execute rule logic
        override protected bool ExcuteRuleLogic(Niffle message)
        {
            if (IsTickMessageEmpty(message)) return false;
            if (TerminateTime == TimeSpan.Zero) return false;

            DateTime Now = DateTime.FromBinary(message.Tick.TimeStamp);

            if (DateTimeZoneCalc.IsTimeAfter(Now, TerminateTime))
            {
                StateManager.SetInitialStateAsync(new State
                    {
                        { State.ISTERMINATETIME, true },
                        { State.TERMINATETIME, message.Tick.TimeStamp }
                    });

                SetActiveState(false);
                return true;
            }
            return false;
        }

        private void SetTerminateTime(DateTime openTime)
        {
            if(TerminateAfterOpen != TimeSpan.Zero)
            {
                TerminateTime = openTime.TimeOfDay;
                TerminateTime = TerminateTime.Add(TerminateAfterOpen);

                //Set CloseAfterOpen to TimeSpan.Zero so that CloseTime is only updated once.
                TerminateAfterOpen = TimeSpan.Zero;
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
                    SetTerminateTime(DateTime.FromBinary(openTime));
            }
        }

        protected override string GetServiceName()
        {
            return nameof(OnTerminateTime);
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

