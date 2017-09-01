using System;
using System.Collections.Generic;
using Niffler.Messaging.Protobuf;
using Niffler.Messaging.RabbitMQ;
using cAlgo.API;
using cAlgo.API.Internals;

namespace Niffler.Services
{
    public class TradeManager : IScalableConsumerService
    {
        public delegate void ExecuteMarketOrder_cAlgo(Trade trade);
        public delegate void PlaceBuyLimitOrder_cAlgo(Trade trade);
        public delegate void PlaceBuyStopOrder_cAlgo(Trade trade);
        public delegate void PlaceSellLimitOrder_cAlgo(Trade trade);
        public delegate void PlaceSellStopOrder_cAlgo(Trade trade);
        public delegate void ModifyPosition_cAlgo(Trade trade);
        public delegate void ModifyOrder_cAlgo(Trade trade);
        public delegate void ClosePosition_cAlgo(Trade trade);
        public delegate void CancelOrder_cAlgo(Trade trade);

        public ExecuteMarketOrder_cAlgo ExecuteMarketOrder { get; set; }
        public PlaceBuyLimitOrder_cAlgo PlaceBuyLimitOrder { get; set; }
        public PlaceBuyStopOrder_cAlgo PlaceBuyStopOrder { get; set; }
        public PlaceSellLimitOrder_cAlgo PlaceSellLimitOrder { get; set; }
        public PlaceSellStopOrder_cAlgo PlaceSellStopOrder { get; set; }
        public ModifyPosition_cAlgo ModifyPosition { get; set; }
        public ClosePosition_cAlgo ClosePosition { get; set; }
        public CancelOrder_cAlgo CancelOrder { get; set; }
        public ModifyOrder_cAlgo ModifyOrder { get; set; }

        //private StopLossManager StopLossManager;
        //private SellLimitOrdersTrader SellLimitOrdersTrader;
        //private BuyLimitOrdersTrader BuyLimitOrdersTrader;
        //SellLimitOrdersTrader = sellLimitOrdersTrader;
        //BuyLimitOrdersTrader = buyLimitOrdersTrader;
        //PositionsManager = new PositionsManager(StateManager);
        //StopLossManager = stopLossManager;
        //FixedTrailingStop = fixedTrailingStop;

        public TradeManager(String exchangeName)
        {
            ExchangeName = exchangeName;
        }

        public override void Init()
        {
            throw new NotImplementedException();
        }

        //Publish State update message
        protected void PublishOnTickEvent(Symbol symbol, string timeStamp, bool isBackTesting = false)
        {
            Tick tick = new Tick()
            {
              Code = symbol.Code,
              Ask = symbol.Ask,
              Bid = symbol.Bid,
              Digits = symbol.Digits,
              PipSize = symbol.PipSize,
              TickSize = symbol.TickSize,
              Spread = symbol.TickSize,
              TimeStamp = timeStamp,
              IsBackTesting = isBackTesting,
              //Positions = 
            };

            Publisher.TradeEvent();
        }

        protected void PublishOnPositionOpened(cAlgo.API.Position _postion, cAlgo.API.Positions _positions)
        {
            //Create position

            //Create the positions object

            //Messaging.Protobuf.Positions positions = new Messaging.Protobuf.Positions()
            //{
            //    Count = _positions.Count,
            //    Position =  
        }

        protected void PublishOnPositionClosed(cAlgo.API.Position _postion, cAlgo.API.Positions _positions)
        {
        }

        protected override void OnMessageReceived(object sender, MessageReceivedEventArgs e)
        {
            DateTime niffleTimeStamp = DateTime.FromBinary(e.Message.Timestamp);
            RoutingKey routingKey = new RoutingKey(e.EventArgs.RoutingKey);
            string action = routingKey.GetAction();
            string _event = routingKey.GetEvent();

            //Check it is a trade operation
            if (routingKey.GetActionAsEnum() != Messaging.RabbitMQ.Action.TRADEOPERATION) return;

            //Single Trade operation messages
            if (e.Message.Type == Niffle.Types.Type.Trade)
            {
                if (e.Message.Trade != null)
                {
                    ExecuteTradeOperation(e.Message.Trade);
                }
            }

            //Multiple Trades operation messages
            if (e.Message.Type == Niffle.Types.Type.Trades)
            {
                if (e.Message.Trades != null)
                {
                    foreach (Trade trade in e.Message.Trades.Trade)
                    {
                        ExecuteTradeOperation(trade);
                    }
                }
                   
            }
        }

        protected void ExecuteTradeOperation(Trade trade)
        {
            switch (trade.TradeType)
            {
                case Trade.Types.TradeType.Buy:
                    ExecuteMarketOrder(trade);
                    break;
                case Trade.Types.TradeType.Buylimitorder:
                    PlaceBuyLimitOrder(trade);
                    break;
                case Trade.Types.TradeType.Buystoporder:
                    PlaceBuyStopOrder(trade);
                    break;
                case Trade.Types.TradeType.Sell:
                    ExecuteMarketOrder(trade);
                    break;
                case Trade.Types.TradeType.Selllimitorder:
                    PlaceSellLimitOrder(trade);
                    break;
                case Trade.Types.TradeType.Sellstoporder:
                    PlaceSellStopOrder(trade);
                    break;
                case Trade.Types.TradeType.Modifyposition:
                    ModifyPosition(trade);
                    break;
                case Trade.Types.TradeType.Modifyorder:
                    ModifyOrder(trade);
                    break;
                case Trade.Types.TradeType.Closeposition:
                    ClosePosition(trade);
                    break;
                case Trade.Types.TradeType.Cancelorder:
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
            ShutDownConsumers();
        }

        public override void Reset()
        {
            throw new NotImplementedException();
        }
    }
}
