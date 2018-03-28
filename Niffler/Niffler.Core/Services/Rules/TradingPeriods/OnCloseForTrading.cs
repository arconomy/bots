using System;
using Niffler.Messaging.RabbitMQ;
using System.Collections.Generic;
using Niffler.Messaging.Protobuf;
using Niffler.Services;
using Niffler.Core.Config;
using Niffler.Common;
using Niffler.Model;

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
            SymbolCode = StrategyConfig.Exchange;
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

            //Listen for state updates for the OpenTime
            StateManager.ListenForStateUpdates();
        }
        
        //Execute rule logic
        override protected bool ExcuteRuleLogic(Niffle message)
        {
            if (IsTickMessageEmpty(message)) return false;
            if (CloseTime == TimeSpan.Zero) return false;

            DateTime Now = DateTime.FromBinary(message.Tick.TimeStamp);

            if (DateTimeZoneCalc.IsTimeAfter(Now, CloseTime))
            {
                StateManager.SetInitialStateAsync(new State
                    {
                        { State.ISOPENTIME, false },
                        { State.CLOSETIME, message.Tick.TimeStamp }
                    });

                SetActiveState(false);
                return true;
            }
            return false;
        }

        private void SetCloseTime(DateTime openTime)
        {
            if (CloseAfterOpen != TimeSpan.Zero)
            {
                CloseTime = openTime.TimeOfDay;
                CloseTime = CloseTime.Add(CloseAfterOpen);

                //Set CloseAfterOpen to TimeSpan.Zero so that CloseTime is only updated once.
                CloseAfterOpen = TimeSpan.Zero;
            }
        }

        override protected void OnServiceNotify(Niffle message,RoutingKey routingKey)
        {
            //Nothing required
        }

        protected override void OnStateUpdate(StateChangedEventArgs stateupdate)
        {
            //Listening for update to OpenTime State
            if (stateupdate.Key == Model.State.OPENTIME)
            {
                if(long.TryParse(stateupdate.Value.ToString(),out long openTime))
                    SetCloseTime(DateTime.FromBinary(openTime));
            }
        }

        protected override string GetServiceName()
        {
            return nameof(OnCloseForTrading);
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
