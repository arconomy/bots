using System;
using System.Collections.Generic;
using Niffler.Messaging.Protobuf;
using Niffler.Messaging.RabbitMQ;
using Niffler.Common;
using cAlgo.API.Internals;

namespace Niffler.Services
{
    public class TestTradeManager : IScalableConsumerService
    {
        public delegate void ExecuteBuyMarketOrder_cAlgo(Trade trade);
        public delegate void ExecuteSellMarketOrder_cAlgo(Trade trade);
        public delegate void PlaceBuyLimitOrder_cAlgo(Trade trade);
        public delegate void PlaceBuyStopOrder_cAlgo(Trade trade);
        public delegate void PlaceSellLimitOrder_cAlgo(Trade trade);
        public delegate void PlaceSellStopOrder_cAlgo(Trade trade);
        public delegate void ModifyPosition_cAlgo(Trade trade);
        public delegate void ModifyOrder_cAlgo(Trade trade);
        public delegate void ClosePosition_cAlgo(Trade trade);
        public delegate void CancelOrder_cAlgo(Trade trade);

        public ExecuteBuyMarketOrder_cAlgo ExecuteBuyMarketOrder { get; set; }
        public ExecuteSellMarketOrder_cAlgo ExecuteSellMarketOrder { get; set; }
        public PlaceBuyLimitOrder_cAlgo PlaceBuyLimitOrder { get; set; }
        public PlaceBuyStopOrder_cAlgo PlaceBuyStopOrder { get; set; }
        public PlaceSellLimitOrder_cAlgo PlaceSellLimitOrder { get; set; }
        public PlaceSellStopOrder_cAlgo PlaceSellStopOrder { get; set; }
        public ModifyPosition_cAlgo ModifyPosition { get; set; }
        public ClosePosition_cAlgo ClosePosition { get; set; }
        public CancelOrder_cAlgo CancelOrder { get; set; }
        public ModifyOrder_cAlgo ModifyOrder { get; set; }

        public TestTradeManager(String exchangeName)
        {
            ExchangeName = exchangeName;
        }

        public override void Init()
        {
            throw new NotImplementedException();
        }

        public void PublishOnTickEvent(Symbol symbol, cAlgo.API.Positions _positions, cAlgo.API.PendingOrders _orders, DateTime timeStamp, bool isBackTesting = false)
        {
            Utils.ParseOpenPositions(_positions, out Positions positions);
            Utils.ParsePendingOrders(_orders, out Orders orders);

            Tick tick = new Tick()
            {
                Code = symbol.Code,
                Ask = symbol.Ask,
                Bid = symbol.Bid,
                Digits = symbol.Digits,
                PipSize = symbol.PipSize,
                TickSize = symbol.TickSize,
                Spread = symbol.TickSize,
                TimeStamp = timeStamp.ToBinary()
            };

            Publisher.TickEvent(tick, positions, orders, isBackTesting);
        }

        public void PublishOnPositionOpened(cAlgo.API.Position _postion, cAlgo.API.Positions _positions, cAlgo.API.PendingOrders _orders, bool isBackTesting = false)
        {
            Utils.ParseOpenPositions(_positions, out Positions positions, _postion.Label);
            Utils.ParsePendingOrders(_orders, out Orders orders);
            Utils.ParseOpenPosition(_postion, out Position position);
            Publisher.PositionOpenedEvent(position, positions, orders, isBackTesting);
        }

        public void PublishOnPositionClosed(cAlgo.API.Position _postion, double closePrice, cAlgo.API.Positions _positions, cAlgo.API.PendingOrders _orders, DateTime closeTime, bool isBackTesting = false)
        {
            Utils.ParseOpenPositions(_positions, out Positions positions);
            Utils.ParsePendingOrders(_orders, out Orders orders);
            Utils.ParseClosedPosition(_postion, closePrice, closeTime, out Position position);
            Publisher.PositionClosedEvent(position, positions, orders, isBackTesting);
        }

        protected override void OnMessageReceived(object sender, MessageReceivedEventArgs e)
        {
            DateTime niffleTimeStamp = DateTime.FromBinary(e.Message.TimeStamp);
            RoutingKey routingKey = new RoutingKey(e.EventArgs.RoutingKey);
            string action = routingKey.GetAction();
            string _event = routingKey.GetEvent();

            switch (routingKey.GetActionAsEnum())
            {
                case Messaging.RabbitMQ.Action.TRADEMANAGEMENT:
                    // Add order labels and action required on filled to State in order to listen for them and take action

                    break;
                case Messaging.RabbitMQ.Action.TRADEOPERATION:
                    // Check State for order labels of filled orders and place Stop loss or Take Profit orders.
                    break;
                default:
                    return;
            };

            //Single Trade operation messages
            if (e.Message.Type == Niffle.Types.Type.Trade)
            {
                if (e.Message.Trade != null)
                {
                    ExecuteTradeOperation(e.Message.Trade);
                }
            }

            //Multiple Trades operation messages
            //if (e.Message.Type == Niffle.Types.Type.Trades)
            //{
            //    if (e.Message.Trades != null)
            //    {
            //        foreach (Trade trade in e.Message.Trades.Trade)
            //        {
            //            ExecuteTradeOperation(trade);
            //        }
            //    }
            //}
        }

        protected void ExecuteTradeOperation(Trade trade)
        {
            switch (trade.TradeAction)
            {
                case Trade.Types.TradeAction.Buy:
                    ExecuteBuyMarketOrder(trade);
                    break;
                case Trade.Types.TradeAction.Buylimitorder:
                    PlaceBuyLimitOrder(trade);
                    break;
                case Trade.Types.TradeAction.Buystoporder:
                    PlaceBuyStopOrder(trade);
                    break;
                case Trade.Types.TradeAction.Sell:
                    ExecuteSellMarketOrder(trade);
                    break;
                case Trade.Types.TradeAction.Selllimitorder:
                    PlaceSellLimitOrder(trade);
                    break;
                case Trade.Types.TradeAction.Sellstoporder:
                    PlaceSellStopOrder(trade);
                    break;
                case Trade.Types.TradeAction.Modifyposition:
                    ModifyPosition(trade);
                    break;
                case Trade.Types.TradeAction.Modifyorder:
                    ModifyOrder(trade);
                    break;
                case Trade.Types.TradeAction.Closeposition:
                    ClosePosition(trade);
                    break;
                case Trade.Types.TradeAction.Cancelorder:
                    CancelOrder(trade);
                    break;
            }
        }

        protected override List<RoutingKey> SetListeningRoutingKeys()
        {
            List<RoutingKey> routingKeys = RoutingKey.Create(Niffler.Messaging.RabbitMQ.Action.TRADEOPERATION).ToList();
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