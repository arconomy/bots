using System;
using System.Collections.Generic;
using Niffler.Messaging.Protobuf;
using Niffler.Messaging.RabbitMQ;
using Niffler.Strategy;

namespace Niffler.Managers
{
    class TradeManager : Consumer
    {
        string StrategyName;
        string StrategyId;
        string Market;
        public delegate void ExecuteMarketOrder_cAlgo(Trade trade);
        public delegate void PlaceBuyLimitOrder_cAlgo(Trade trade);
        public delegate void PlaceBuyStopOrder_cAlgo(Trade trade);
        public delegate void PlaceSellLimitOrder_cAlgo(Trade trade);
        public delegate void PlaceSellStopOrder_cAlgo(Trade trade);
        public delegate void ModifyPosition_cAlgo(Trade trade);
        public delegate void ModifyOrder_cAlgo(Trade trade);
        public delegate void ClosePosition_cAlgo(Trade trade);
        public delegate void CancelOrder_cAlgo(Trade trade);

        public ExecuteMarketOrder_cAlgo ExecuteMarketOrder;
        public PlaceBuyLimitOrder_cAlgo PlaceBuyLimitOrder;
        public PlaceBuyStopOrder_cAlgo PlaceBuyStopOrder;
        public PlaceSellLimitOrder_cAlgo PlaceSellLimitOrder;
        public PlaceSellStopOrder_cAlgo PlaceSellStopOrder;
        public ModifyPosition_cAlgo ModifyPosition;
        public ClosePosition_cAlgo ClosePosition;
        public CancelOrder_cAlgo CancelOrder;
        public ModifyOrder_cAlgo ModifyOrder;

        //private StopLossManager StopLossManager;
        //private SellLimitOrdersTrader SellLimitOrdersTrader;
        //private BuyLimitOrdersTrader BuyLimitOrdersTrader;
        //SellLimitOrdersTrader = sellLimitOrdersTrader;
        //BuyLimitOrdersTrader = buyLimitOrdersTrader;
        //PositionsManager = new PositionsManager(StateManager);
        //StopLossManager = stopLossManager;
        //FixedTrailingStop = fixedTrailingStop;

        public TradeManager(StrategyConfiguration strategyConfig) : base(strategyConfig)
        {
            StrategyName = strategyConfig.Name;
        }

        public override object Clone()
        {
            return new TradeManager(StrategyConfig);
        }

        public override bool Init()
        {
            if (!StrategyConfig.Config.TryGetValue(StrategyConfiguration.STRATEGYID, out StrategyId)) return false;
            if (!StrategyConfig.Config.TryGetValue(StrategyConfiguration.EXCHANGE, out Market)) return false;

            return true;
        }

        public override void MessageReceived(MessageReceivedEventArgs e)
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

        protected override List<RoutingKey> GetListeningRoutingKeys()
        {
            List<RoutingKey> routingKeys = RoutingKey.Create(Niffler.Messaging.RabbitMQ.Action.TRADEOPERATION).ToList();
            return routingKeys;
        }

        public override object Clone(StrategyConfiguration strategyConfig, bool autoscale = false, int timeout = 10, ushort prefetchCount = 1, bool autoAck = true, IDictionary<string, object> queueArgs = null)
        {
            return new TradeManager(strategyConfig);
        }
    }
}
