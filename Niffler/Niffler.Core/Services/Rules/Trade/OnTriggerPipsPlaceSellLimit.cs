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
    class OnTriggerPipsPlaceSellLimit : IRule
    {
        //Dependent state variables
        private string SymbolCode;
        private double OpenPrice;

        //Instance variables
        private bool ExecuteOnlyOnce;
        private int NumberOfOrders;
        private double TriggerPipsFromTradeOpenPrice;
        private double TakeProfitPips;
        private double OrderSpacingMinPips;
        private double OrderSpacingMaxPips;
        private double OrderSpacingIncrementPips;
        private double NumberOfOrdersBeforeSpacingAdjustment;
        private double OrderSpacingIncrementAdjustmentPips;
        
        public OnTriggerPipsPlaceSellLimit(StrategyConfiguration StrategyConfig, RuleConfiguration ruleConfig) : base(StrategyConfig, ruleConfig) { }

        //Retreive data from config to initialise the rule
        public override void Init()
        {
            SymbolCode = GetSymbolCode();

            //Get Rule Config params
            ExecuteOnlyOnce = GetRuleConfigBoolParam(RuleConfiguration.EXECUTEONLYONCE);
            NumberOfOrders = GetRuleConfigIntegerParam(RuleConfiguration.NUMBEROFORDERS);
            TriggerPipsFromTradeOpenPrice = GetRuleConfigDoubleParam(RuleConfiguration.TRIGGERPIPSFROMTRADEOPENPRICE);
            TakeProfitPips = GetRuleConfigDoubleParam(RuleConfiguration.TAKEPROFIPIPS);
            OrderSpacingMinPips = GetRuleConfigDoubleParam(RuleConfiguration.ORDERSPACINGMINPIPS);
            OrderSpacingMaxPips = GetRuleConfigDoubleParam(RuleConfiguration.ORDERSPACINGMAXPIPS);
            OrderSpacingIncrementPips = GetRuleConfigDoubleParam(RuleConfiguration.ORDERSPACINGINCADJPIPS);
            NumberOfOrdersBeforeSpacingAdjustment = GetRuleConfigIntegerParam(RuleConfiguration.NUMORDERSBEFORESPACINGADJ);
            OrderSpacingIncrementAdjustmentPips = GetRuleConfigIntegerParam(RuleConfiguration.ORDERSPACINGINCADJPIPS);
            
        //Listen for state updates for
        StateManager.ListenForStateUpdates();
        }
        
        //Execute rule logic
        override protected bool ExcuteRuleLogic(Niffle message)
        {
            if (IsTickMessageEmpty(message)) return false;
            if (TriggerPipsFromTradeOpenPrice == 0.0 ) return false; //trigger points should be set when initialised
            if (OpenPrice == 0.0) return false; //Open price should be set once activated

            double SellTriggerPrice = TriggerPipsFromTradeOpenPrice * message.Tick.PipSize + OpenPrice;
            if(SellTriggerPrice < message.Tick.Bid)
            {
                for (int count = 0;count < NumberOfOrders; count++)
                {
                    //Place sell limit orders
                    TradePublisher.PlaceSellLimitOrder(SymbolCode,StrategyId + "-" + count,calcVolume();
                }
            }

            //Set inactive if only executing once
            // SetActiveState(false);

            //return true to publish a success Service Notify message
            //      return true;
            //  }



            //double BuyTriggerPrice = TriggerPips * message.Tick.PipSize + OpenPrice;
            //if (BuyTriggerPrice > message.Tick.Bid)
            //{
            //    //Place buy limit orders


            //    //Set inactive if only executing once
            //    SetActiveState(false);

            //    //return true to publish a success Service Notify message
            //    return true;
            //}

            return false;
        }

        override protected void OnServiceNotify(Niffle message,RoutingKey routingKey)
        {
            //Nothing required
        }

        protected override void OnStateUpdate(StateChangedEventArgs stateupdate)
        {
            //Listening for update to OpenTime State
            if (stateupdate.Key == Model.State.OPENPRICE)
            {
                if (double.TryParse(stateupdate.Value.ToString(), out double openPrice))
                    if(openPrice > 0.0)
                        OpenPrice = openPrice;
            }
        }

        protected override string GetServiceName()
        {
            return nameof(OnTriggerPipsPlaceSellLimit);
        }

        protected override void AddListeningRoutingKeys(ref List<RoutingKey> routingKeys)
        {
            //Listen for OnTick
            routingKeys.Add(RoutingKey.Create(Source.WILDCARD, Messaging.RabbitMQ.Action.WILDCARD, Event.ONTICK));
        }

        public override void Reset()
        {
            //Wait until OpenForTrading notification before becoming active - set in config
            SetActiveState(false);
        }
    }
}
