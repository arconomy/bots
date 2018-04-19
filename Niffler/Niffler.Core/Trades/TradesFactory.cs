using Niffler.Messaging.Protobuf;

namespace Niffler.Core.Trades
{
    public class TradesFactory
    {

        public Trade CreateSellLimitTrade(string symbolCode, string label, double volume, double targetEntryPrice, bool isLinkedTrade, string linkedParentTrade)
        {
            if (isLinkedTrade)
            {
                Trade trade = CreateSellLimitTrade(symbolCode, label, volume, targetEntryPrice);
                trade.LinkedTradeLabel = label;
                trade.IsLinkedTrade = isLinkedTrade;
                return trade;
            }
            else
            {
                return CreateSellLimitTrade(symbolCode, label, volume, targetEntryPrice);
            }
        }

        public Trade CreateSellLimitTrade(string symbolCode, string label, double volume, double targetEntryPrice)
        {
            return new Trade()
            {
                TradeAction = Messaging.Protobuf.Trade.Types.TradeAction.Selllimitorder,
                Order = new Order()
                {
                    OrderType = Order.Types.OrderType.Limit,
                    TradeType = Order.Types.TradeType.Sell,
                    SymbolCode = symbolCode,
                    Label = label,
                    Volume = volume,
                    TargetEntryPrice = targetEntryPrice,
                }
            };
        }

        public Trade CreateSellStopTrade(string symbolCode, string label, double volume, double targetEntryPrice, bool isLinkedTrade, string linkedParentTrade)
        {
            if (isLinkedTrade)
            {
                Trade trade = CreateSellStopTrade(symbolCode, label, volume, targetEntryPrice);
                trade.LinkedTradeLabel = label;
                trade.IsLinkedTrade = isLinkedTrade;
                return trade;
            }
            else
            {
                return CreateSellStopTrade(symbolCode, label, volume, targetEntryPrice);
            }
        }

        public Trade CreateSellStopTrade(string symbolCode, string label, double volume, double targetEntryPrice)
        {
            return new Trade()
            {
                TradeAction = Messaging.Protobuf.Trade.Types.TradeAction.Sellstoporder,
                Order = new Order()
                {
                    OrderType = Order.Types.OrderType.Stop,
                    TradeType = Order.Types.TradeType.Sell,
                    SymbolCode = symbolCode,
                    Label = label,
                    Volume = volume,
                    TargetEntryPrice = targetEntryPrice,
                }
            };
        }

        public Trade CreateBuyLimitTrade(string symbolCode, string label, double volume, double targetEntryPrice, bool isLinkedTrade, string linkedParentTrade)
        {
            if(isLinkedTrade)
            {
                Trade trade = CreateBuyLimitTrade(symbolCode, label, volume, targetEntryPrice);
                trade.LinkedTradeLabel = label;
                trade.IsLinkedTrade = isLinkedTrade;
                return trade;
            }
            else
            {
                return CreateBuyLimitTrade(symbolCode, label, volume, targetEntryPrice);
            }
        }
        
        public Trade CreateBuyLimitTrade(string symbolCode, string label, double volume, double targetEntryPrice)
        {
            return new Trade()
            {
                TradeAction = Messaging.Protobuf.Trade.Types.TradeAction.Buylimitorder,
                Order = new Order()
                {
                    OrderType = Order.Types.OrderType.Limit,
                    TradeType = Order.Types.TradeType.Buy,
                    SymbolCode = symbolCode,
                    Label = label,
                    Volume = volume,
                    TargetEntryPrice = targetEntryPrice,
                },
            };
        }

        public Trade CreateBuyStopTrade(string symbolCode, string label, double volume, double targetEntryPrice, bool isLinkedTrade, string linkedParentTrade)
        {
            if (isLinkedTrade)
            {
                Trade trade = CreateBuyStopTrade(symbolCode, label, volume, targetEntryPrice);
                trade.LinkedTradeLabel = label;
                trade.IsLinkedTrade = isLinkedTrade;
                return trade;
            }
            else
            {
                return CreateBuyStopTrade(symbolCode, label, volume, targetEntryPrice);
            }
        }

        public Trade CreateBuyStopTrade(string symbolCode, string label, double volume, double targetEntryPrice)
        {
            return new Trade()
            {
                TradeAction = Messaging.Protobuf.Trade.Types.TradeAction.Buylimitorder,
                Order = new Order()
                {
                    OrderType = Order.Types.OrderType.Stop,
                    TradeType = Order.Types.TradeType.Buy,
                    SymbolCode = symbolCode,
                    Label = label,
                    Volume = volume,
                    TargetEntryPrice = targetEntryPrice,
                }
            };
        }

        public Trade CreateSellTrade(string symbolCode, string label, double volume, double targetEntryPrice, bool isLinkedTrade, string linkedParentTrade)
        {
            if (isLinkedTrade)
            {
                Trade trade = CreateSellTrade(symbolCode, label, volume, targetEntryPrice);
                trade.LinkedTradeLabel = label;
                trade.IsLinkedTrade = isLinkedTrade;
                return trade;
            }
            else
            {
                return CreateSellTrade(symbolCode, label, volume, targetEntryPrice);
            }
        }

        public Trade CreateSellTrade(string symbolCode, string label, double volume, double targetEntryPrice)
        {
            return new Trade()
            {
                TradeAction = Messaging.Protobuf.Trade.Types.TradeAction.Sell,
                Order = new Order()
                {
                    OrderType = Order.Types.OrderType.Market,
                    TradeType = Order.Types.TradeType.Sell,
                    SymbolCode = symbolCode,
                    Label = label,
                    Volume = volume
                }
            };
        }

        public Trade CreateBuyTrade(string symbolCode, string label, double volume, double targetEntryPrice, bool isLinkedTrade, string linkedParentTrade)
        {
            if (isLinkedTrade)
            {
                Trade trade = CreateBuyTrade(symbolCode, label, volume, targetEntryPrice);
                trade.LinkedTradeLabel = label;
                trade.IsLinkedTrade = isLinkedTrade;
                return trade;
            }
            else
            {
                return CreateBuyTrade(symbolCode, label, volume, targetEntryPrice);
            }
        }

        public Trade CreateBuyTrade(string symbolCode, string label, double volume, double targetEntryPrice)
        {
            return new Trade()
            {
                TradeAction = Messaging.Protobuf.Trade.Types.TradeAction.Buy,
                Order = new Order()
                {
                    OrderType = Order.Types.OrderType.Market,
                    TradeType = Order.Types.TradeType.Buy,
                    SymbolCode = symbolCode,
                    Label = label,
                    Volume = volume
                }
            };
        }


       


        


    }
}