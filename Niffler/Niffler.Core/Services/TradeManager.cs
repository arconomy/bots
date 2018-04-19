using System;
using System.Collections.Generic;
using Niffler.Messaging.Protobuf;
using Niffler.Messaging.RabbitMQ;
using Niffler.Common;
using cAlgo.API.Internals;
using Niffler.Core.Model;
using Niffler.Core.Config;

namespace Niffler.Services
{
    public class TradeManager : IScalableConsumerService
    {
        private StateManager StateManager;
        private String EntityName = "TradeManager";
        protected string StrategyId;
        protected StrategyConfiguration StrategyConfig;
        protected string BrokerId;

        public TradeManager(StrategyConfiguration strategyConfig, StateManager stateManager)
        {
            StateManager = stateManager;

            ExchangeName = StrategyConfig.Exchange;
            if (String.IsNullOrEmpty(ExchangeName)) IsInitialised = false;
        }

        public override void Init()
        {
            throw new NotImplementedException();
        }

        protected override void OnMessageReceived(object sender, MessageReceivedEventArgs e)
        {
            DateTime niffleTimeStamp = DateTime.FromBinary(e.Message.TimeStamp);
            RoutingKey routingKey = new RoutingKey(e.EventArgs.RoutingKey);
            string action = routingKey.GetAction();
            string _event = routingKey.GetEvent();
            Trade trade = e.Message.Trade;

            switch (routingKey.GetActionAsEnum())
            {
                case Messaging.RabbitMQ.Action.TRADEMANAGEMENT:
                    //Save the trade to execute against the Linked Trade label
                    StateManager.UpdateStateLinkedTradeAsync(trade.LinkedTradeLabel, trade.Order.Label, trade);
                    break;
            }

            // Need to have another transforming service if FIX service only publishes raw messages
            // Need to have a service that listens for routing key FIXServiceName.*.*
            // This service will then have to unpack the FIX API message and construct the appropriate msg and routing key

            switch (routingKey.GetEventAsEnum())
            {
                case Messaging.RabbitMQ.Event.ONPOSITIONCLOSED:
                    switch (e.Message.Trade.Order.StateChange)
                    {
                        case Order.Types.StateChange.Filled: //The stateChange should be used to set the routing key and not needed here.
                            //Check if this is a linked trade 
                            if (trade.IsLinkedTrade)
                            {
                                //Save the state of the filled linked trade
                                StateManager.UpdateStateLinkedTradeAsync(trade.LinkedTradeLabel, trade.Order.Label, trade);

                                //Find all other linked trades and cancel them
                                StateManager.FindAllLinkedTradesExcludingAsync(trade.LinkedTradeLabel, trade.Order.Label, CancelTradeOperation);
                            }
                            //This is a masker trade therefore place any linked SL and TP trades
                            else
                            {
                                //Find all linked trades and excute them
                                StateManager.FindAllLinkedTradesAsync(trade.LinkedTradeLabel, trade.Order.PosMaintRptID, ExecuteTradeOperation);
                            }
                        break;
                        case Order.Types.StateChange.Canceled:
                            //If this is a masker trade then cancel any linked SL and TP trades
                            if (!trade.IsLinkedTrade)
                            {
                                //Find all linked trades and cancel them
                                StateManager.FindAllLinkedTradesAsync(trade.LinkedTradeLabel, trade.Order.PosMaintRptID, CancelTradeOperation);
                            }
                            break;
                    }
                
                    //Order filled
                    if (e.Message.Trade.Order.StateChange == Order.Types.StateChange.Filled)
                    {
                       

                    }
                    break;
                default:
                    return;
            };
        }

        protected void CancelTradeOperation(Trade trade)
        {
            trade.TradeAction = Trade.Types.TradeAction.Cancelorder;
            ExecuteTradeOperation(trade);
        }

        protected void ExecuteTradeOperation(Trade trade)
        {
            Publisher.TradeOperation(trade,EntityName);
        }

        protected override List<RoutingKey> SetListeningRoutingKeys()
        {
            List<RoutingKey> routingKeys = RoutingKey.Create(Niffler.Messaging.RabbitMQ.Event.ONORDERCANCELLED).ToList();
            routingKeys.Add(RoutingKey.Create(Niffler.Messaging.RabbitMQ.Event.ONORDERMODIFIED));
            routingKeys.Add(RoutingKey.Create(Niffler.Messaging.RabbitMQ.Event.ONORDERPLACED));
            routingKeys.Add(RoutingKey.Create(Niffler.Messaging.RabbitMQ.Event.ONPOSITIONCLOSED));
            routingKeys.Add(RoutingKey.Create(Niffler.Messaging.RabbitMQ.Event.ONPOSITIONMODIFIED));
            routingKeys.Add(RoutingKey.Create(Niffler.Messaging.RabbitMQ.Event.ONPOSITIONOPENED));
            routingKeys.Add(RoutingKey.Create(Niffler.Messaging.RabbitMQ.Action.TRADEMANAGEMENT));
            return routingKeys;
        }

        public override void ShutDown()
        {
            ShutDownService();
        }

        public override void Reset()
        {
            throw new NotImplementedException();
        }
    }
}
