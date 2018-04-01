using System;
using System.Collections.Generic;
using Niffler.Messaging.Protobuf;
using Niffler.Messaging.RabbitMQ;
using Niffler.Common;
using cAlgo.API.Internals;
using Niffler.Core.Model;
using Niffler.Core.Trades;
using Niffler.Core.Config;

namespace Niffler.Services
{
    public class TradeManager : IScalableConsumerService
    {
        private StateManager StateManager;
        private TradePublisher TradePublisher;
        private TradeUtils TradeUtils;
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
                    StateManager.UpdateStateLinkedTradeAsync(trade.LinkedTradeLabel, trade.Order.Label,trade);

                    break;
                case Messaging.RabbitMQ.Action.TRADEOPERATION:
                    switch (e.Message.Trade.Order.StateChange)
                    {
                        case Order.Types.StateChange.Filled:
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
            List<RoutingKey> routingKeys = RoutingKey.Create(Niffler.Messaging.RabbitMQ.Action.TRADEOPERATION).ToList();
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
