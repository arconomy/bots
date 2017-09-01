using System;
using System.Collections.Generic;
using Google.Protobuf.Collections;
using Niffler.Messaging.Protobuf;
using Niffler.Messaging.RabbitMQ;

namespace Niffler.Rules
{
    class OpenTimeUseBollingerBand : IRule
    {
        public OpenTimeUseBollingerBand(int priority) : base(priority) { }

        //If rule should only execute when bot is trading return TRUE, default is FALSE
        protected override bool IsTradingRule()
        {
            return true;
        }

        //After CLose time set hard stop losses at last position entry price with Buffer
        override protected bool ExcuteRuleLogic()
        {
            if (MarketInfo.IsBotTradingOpen())
            {
                //SellLimitOrdersTrader.SetBollingerBandDefault(Bot.MarketData.GetSeries());

                //Price moves TriggerOrderPlacementPips UP from open then look to set SELL LimitOrders
                if (BotState.OpenPrice + SellLimitOrdersTrader.EntryTriggerOrderPlacementPips < Bot.Symbol.Bid)
                {
                    if(SellLimitOrdersTrader.IsOutSideBollingerBand())
                    {
                        SellLimitOrdersTrader.PlaceSellLimitOrders();
                        BotState.OrdersPlaced = true;
                        ExecuteOnceOnly();
                        return true;
                    }
                   
                }
                //Price moves 5pts DOWN from open then look to set BUY LimitOrders
                else if (BotState.OpenPrice - BuyLimitOrdersTrader.EntryTriggerOrderPlacementPips > Bot.Symbol.Ask)
                {
                    if (BuyLimitOrdersTrader.IsOutSideBollingerBand())
                    {
                        BuyLimitOrdersTrader.PlaceBuyLimitOrders();
                        BotState.OrdersPlaced = true;
                        ExecuteOnceOnly();
                        return true;
                    }
                }
            }
            return false;
        }

        override protected void Reset()
        {
            // reset any botstate variables to the state prior to executing rule
            SellLimitOrdersTrader.ResetBollingerBand();
            BuyLimitOrdersTrader.ResetBollingerBand();
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
