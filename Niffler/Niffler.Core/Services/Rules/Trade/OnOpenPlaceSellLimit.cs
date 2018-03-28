using System;
using Niffler.Messaging.RabbitMQ;
using System.Collections.Generic;
using Niffler.Messaging.Protobuf;
using Niffler.Services;
using Niffler.Core.Config;
using Niffler.Common;
using Niffler.Model;
using Niffler.Core.Trades;

namespace Niffler.Rules.TradingPeriods
{
    class OnOpenPlaceSellLimit : IRule
    {
        //Dependent state variables
        private string SymbolCode;
        private double OpenPrice;

        //Instance variables
        private bool ExecuteOnlyOnce;
        private int NumberOfOrders;
        private double EntryPipsFromTradeOpenPrice;
        private double TakeProfitPips;

        private bool EnableOrderSpacing;
        private double OrderSpacingBasePips;
        private double OrderSpacingMaxPips;
        private double OrderSpacingIncrementPips;
        private double IncrementSpacingAferOrders;

        private bool EnableVolumeIncrease;
        private bool UseVolumeMultiplier;
        private double VolumeBase;
        private double VolumeMax;
        private double VolumeMultiplier;
        private double VolumeIncrement;
        private double IncreaseVolumeAfterOrders;
        
        public OnOpenPlaceSellLimit(StrategyConfiguration StrategyConfig, RuleConfiguration ruleConfig) : base(StrategyConfig, ruleConfig) { }

        //Retreive data from config to initialise the rule
        public override void Init()
        {
            SymbolCode = GetSymbolCode();

            //Get Rule Config params
            ExecuteOnlyOnce = GetRuleConfigBoolParam(RuleConfiguration.EXECUTEONLYONCE);
            NumberOfOrders = GetRuleConfigIntegerParam(RuleConfiguration.NUMBEROFORDERS);
            EntryPipsFromTradeOpenPrice = GetRuleConfigDoubleParam(RuleConfiguration.ENTRYPIPSFROMTRADEOPENPRICE);
            TakeProfitPips = GetRuleConfigDoubleParam(RuleConfiguration.TAKEPROFIPIPS);
            EnableOrderSpacing = GetRuleConfigBoolParam(RuleConfiguration.ENABLEORDERSPACING);
            EnableVolumeIncrease = GetRuleConfigBoolParam(RuleConfiguration.ENABLEVOLUMEINCREASE);

            if (EnableOrderSpacing)
            {
                OrderSpacingBasePips = GetRuleConfigDoubleParam(RuleConfiguration.ORDERSPACINGBASEPIPS);
                OrderSpacingMaxPips = GetRuleConfigDoubleParam(RuleConfiguration.ORDERSPACINGMAXPIPS);
                OrderSpacingIncrementPips = GetRuleConfigDoubleParam(RuleConfiguration.ORDERSPACINGINCPIPS);
                IncrementSpacingAferOrders = GetRuleConfigIntegerParam(RuleConfiguration.INCREMENTSPACINGAFTER);
                TradeUtils.OrderSpacingCalculator = new OrderSpacingCalculator(EnableOrderSpacing, 
                                                                                TradeUtils.CalcPipsForBroker(OrderSpacingBasePips),
                                                                                TradeUtils.CalcPipsForBroker(OrderSpacingMaxPips),
                                                                                TradeUtils.CalcPipsForBroker(OrderSpacingIncrementPips),
                                                                                IncrementSpacingAferOrders);
            }

            if (EnableVolumeIncrease)
            {
                VolumeBase = GetRuleConfigDoubleParam(RuleConfiguration.VOLUMEBASE);
                VolumeMax = GetRuleConfigDoubleParam(RuleConfiguration.VOLUMEMAX);
                IncreaseVolumeAfterOrders = GetRuleConfigIntegerParam(RuleConfiguration.INCREASEVOLUMEAFTER);
                UseVolumeMultiplier = GetRuleConfigBoolParam(RuleConfiguration.USEVOLUMEMULTIPLIER);
                if (UseVolumeMultiplier)
                {
                    VolumeMultiplier = GetRuleConfigDoubleParam(RuleConfiguration.VOLUMEMULTIPLIER);
                    TradeUtils.TradeVolumeCalculator = new TradeVolumeCalculator(EnableVolumeIncrease,UseVolumeMultiplier, VolumeBase,VolumeMax,VolumeMultiplier,IncreaseVolumeAfterOrders);
                }
                else
                {
                    VolumeIncrement = GetRuleConfigIntegerParam(RuleConfiguration.VOLUMEINCREMENT);
                    TradeUtils.TradeVolumeCalculator = new TradeVolumeCalculator(EnableOrderSpacing,UseVolumeMultiplier, VolumeBase, VolumeMax, VolumeIncrement, IncreaseVolumeAfterOrders);
                }
            }
            
            //Listen for state updates for this strategy
            StateManager.ListenForStateUpdates();
        }

        //Execute rule logic
        //Place Sell Limit orders when notified by OnOpenForTrading rule
        override protected bool ExcuteRuleLogic(Niffle message)
        {
            if (IsTickMessageEmpty(message)) return false;
            if (EntryPipsFromTradeOpenPrice == 0.0 ) return false; //trigger points should be set when initialised
            if (OpenPrice == 0.0) return false; //Open price should be set once activated

                for (int count = 0;count < NumberOfOrders; count++)
                {
                    TradePublisher.PlaceSellLimit(SymbolCode, StrategyId + "-" + count,  TradeUtils.CalculateVolume(count), OpenPrice + TradeUtils.CalcPipsForBroker(EntryPipsFromTradeOpenPrice));
                }
            
            //Set inactive if only executing once
            SetActiveState(false);

            //return true to publish a success Service Notify message
            return true;
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
            return nameof(OnOpenPlaceSellLimit);
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
