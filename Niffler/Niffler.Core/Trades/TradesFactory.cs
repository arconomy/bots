using Niffler.Messaging.Protobuf;

namespace Niffler.Core.Trades
{
    public class TradesFactory
    {
        public Trade CreateSellLimitOrderTrade(string symbolCode, string label, int volume, double targetEntryPrice)
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

        public Trade CreateSellStopOrderTrade(string symbolCode, string label, int volume, double targetEntryPrice)
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

        public Trade CreateBuyLimitOrderTrade(string symbolCode, string label, int volume, double targetEntryPrice)
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
                }
            };
        }

        public Trade CreateBuyStopOrderTrade(string symbolCode, string label, int volume, double targetEntryPrice)
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

        public Trade CreateSellOrderTrade(string symbolCode, string label, int volume, double targetEntryPrice)
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

        public Trade CreateBuyOrderTrade(string symbolCode, string label, int volume, double targetEntryPrice)
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