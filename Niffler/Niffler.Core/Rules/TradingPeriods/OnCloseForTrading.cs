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

            //Wait until OpenForTrading notifies before becoming active
            IsActive = false; 
        }
        
        //Execute rule logic
        override protected bool ExcuteRuleLogic(Niffle message)
        {
            if (IsTickMessageEmpty(message)) return false;
            if (CloseTime == TimeSpan.Zero) return false;

            DateTime Now = DateTime.FromBinary(message.Tick.TimeStamp);

            if (DateTimeZoneCalc.IsTimeAfter(Now, CloseTime))
            {
                StateManager.UpdateState(new State
                    {
                        { State.ISOPENTIME, false },
                        { State.CLOSETIME, message.Tick.TimeStamp }
                    });

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
                CloseTime = CloseTime.Add(CloseAfterOpen);

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

        protected override void OnStateUpdate(StateReceivedEventArgs stateupdate)
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

        protected override List<RoutingKey> SetListeningRoutingKeys()
        {
            //Listen for OnTick
            List < RoutingKey > routingKeys = RoutingKey.Create(Source.WILDCARD, Messaging.RabbitMQ.Action.WILDCARD, Event.ONTICK).ToList();

            //Listen for successful Service execution Notification from OnOpenForTrading
            routingKeys.Add(RoutingKey.Create(nameof(OnOpenForTrading), Messaging.RabbitMQ.Action.NOTIFY, Event.WILDCARD));

            //Listen for Update State Notification from StateManager for the Open Time
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
