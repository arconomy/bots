using Niffler.Messaging.RabbitMQ;
using System.Collections.Generic;
using Niffler.Messaging.Protobuf;
using Niffler.Core.Config;
using Niffler.Model;
using Niffler.Core.Trades;
using Niffler.Core.Model;

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

        private bool EnableDynamicOrderSpacing;
        private double OrderSpacingBasePips;
        private double OrderSpacingMaxPips;
        private double OrderSpacingIncrementPips;
        private double IncrementSpacingAfterOrders;
 
        public OnOpenPlaceSellLimit(StrategyConfiguration StrategyConfig, RuleConfiguration ruleConfig) : base(StrategyConfig, ruleConfig) { }

        //Retreive data from config to initialise the rule
        public override void Init()
        {
            SymbolCode = GetSymbolCode();

            //Get Rule Config params
            ExecuteOnlyOnce = GetRuleConfigBoolParam(RuleConfiguration.EXECUTEONLYONCE);
            NumberOfOrders = GetRuleConfigIntegerParam(RuleConfiguration.NUMBEROFORDERS);
            EntryPipsFromTradeOpenPrice = TradeUtils.CalcPipsForBroker(GetRuleConfigDoubleParam(RuleConfiguration.ENTRYPIPSFROMTRADEOPENPRICE));
            TakeProfitPips = TradeUtils.CalcPipsForBroker(GetRuleConfigDoubleParam(RuleConfiguration.TAKEPROFIPIPS));
           
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

            double EntryPrice = TradeUtils.AddPipsToPrice(OpenPrice, EntryPipsFromTradeOpenPrice);

                for (int orderNumber = 1; orderNumber <= NumberOfOrders; orderNumber++)
                {
                    TradePublisher.PlaceSellLimit(SymbolCode, 
                                                    StrategyId + "-" + orderNumber,  
                                                    TradeUtils.CalculateNextOrderVolume(orderNumber),
                                                    TradeUtils.AddPipsToPrice(EntryPrice,TradeUtils.CalculateNextEntryPips(orderNumber)));
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
