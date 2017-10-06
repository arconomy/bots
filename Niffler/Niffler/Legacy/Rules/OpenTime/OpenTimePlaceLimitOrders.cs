using System;
using System.Collections.Generic;
using Google.Protobuf.Collections;
using Niffler.Messaging.Protobuf;
using Niffler.Messaging.RabbitMQ;

namespace Niffler.Rules
{
    class OpenTimePlaceLimitOrders : IRule
    {
        public OpenTimePlaceLimitOrders(int priority) : base(priority) { }

        //If rule should only execute when bot is trading return TRUE, default is FALSE
        protected override bool IsTradingRule()
        {
            return true;
        }

        //Get the Opening price for the trading period
        override protected bool ExcuteRuleLogic()
        {
            if (BotState.IsOpenTime)
            {
                //Price moves TriggerOrderPlacementPips UP from open then look to set SELL LimitOrders
                if (BotState.OpenPrice + SellLimitOrdersTrader.EntryTriggerOrderPlacementPips < Bot.Symbol.Bid)
                {
                    SellLimitOrdersTrader.PlaceSellLimitOrders();
                    BotState.OrdersPlaced = true;
                    ExecuteOnceOnly();
                    return true;
                }
                //Price moves 5pts DOWN from open then look to set BUY LimitOrders
                else if (BotState.OpenPrice - BuyLimitOrdersTrader.EntryTriggerOrderPlacementPips > Bot.Symbol.Ask)
                {
                    BuyLimitOrdersTrader.PlaceBuyLimitOrders();
                    BotState.OrdersPlaced = true;
                    ExecuteOnceOnly();
                    return true;
                }
            }
            return false;
        }

        // reset any botstate variables to the state prior to executing rule
        override protected void Reset()
        {
            BotState.OrdersPlaced = false;
        }

        // report stats on rule execution 
        // e.g. execution rate, last position rule applied to, number of positions impacted by rule
        override public MapField<String, String> GetLastExecutionData()
        {
            return new MapField<string, string> { { "result", "success" } };

        }

        //create name of Rule Topic for Pub/Sub
        public override string GetPubSubTopicName()
        {
            return this.GetType().Name;
        }

        protected override string GetServiceName()
        {
            throw new NotImplementedException();
        }

        protected override bool ExcuteRuleLogic(Niffle message)
        {
            throw new NotImplementedException();
        }

        protected override List<RoutingKey> SetListeningRoutingKeys()
        {
            throw new NotImplementedException();
        }

        protected override void OnServiceNotify(Niffle message, RoutingKey routingKey)
        {
            throw new NotImplementedException();
        }

        protected override void OnStateUpdate(Niffle message, RoutingKey routingKey)
        {
            throw new NotImplementedException();
        }

        public override object Clone()
        {
            throw new NotImplementedException();
        }

        public override bool Init()
        {
            throw new NotImplementedException();
        }
    }
}
