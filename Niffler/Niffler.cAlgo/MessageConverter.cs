﻿using System;
using Niffler.Messaging.Protobuf;
using cAlgo.API;

namespace Niffler.cAlgoClient
{
    public class MessageConverter
    {
        public static Order.Types.TradeType ParseOrderTradeType(TradeType _tradeType)
        {
            switch (_tradeType)
            {
                case TradeType.Buy:
                    return Order.Types.TradeType.Buy;
                case TradeType.Sell:
                    return Order.Types.TradeType.Sell;
                default:
                    return new Order.Types.TradeType(); //Return something
            }
        }

        public static Messaging.Protobuf.Position.Types.TradeType ParsePositionTradeType(TradeType _tradeType)
        {
            switch (_tradeType)
            {
                case TradeType.Buy:
                    return Messaging.Protobuf.Position.Types.TradeType.Buy;
                case TradeType.Sell:
                    return Messaging.Protobuf.Position.Types.TradeType.Sell;
                default:
                    return new Messaging.Protobuf.Position.Types.TradeType(); //Return something
            }
        }

        public static Order.Types.OrderType ParseOrderType(PendingOrderType orderType)
        {
            switch (orderType)
            {
                case PendingOrderType.Limit:
                    return Order.Types.OrderType.Limit;
                case PendingOrderType.Stop:
                    return Order.Types.OrderType.Stop;
                default:
                    return new Order.Types.OrderType(); //Return something
            }
        }

        public static bool ParsePendingOrders(PendingOrders pendingOrders, out Orders orders)
        {
            try
            {
                int index = 0;
                orders = new Orders();

                foreach (PendingOrder p in pendingOrders)
                {

                    Order order = new Order()
                    {
                        Id = p.Id,
                        Label = p.Label,
                        TargetEntryPrice = p.TargetPrice,
                        StopLossPips = (double)p.StopLossPips,
                        StopLossPrice = (double)p.StopLoss,
                        TakeProfitPips = (double)p.TakeProfitPips,
                        TakeProfitPrice = (double)p.TakeProfit,
                        Volume = p.Volume,
                        TradeType = ParseOrderTradeType(p.TradeType),
                        OrderType = ParseOrderType(p.OrderType)
                    };
                    orders.Order[index] = order;
                    index++;
                }
                orders.Count = index;
            }
            catch (Exception e)
            {
                Console.WriteLine("FAILED to parse Orders: " + e.ToString());
                orders = null;
                return false;
            }
            return true;
        }

        public static bool ParseOpenPositions(cAlgo.API.Positions _positions, out Messaging.Protobuf.Positions positions, string openedPositionLabel = "")
        {
            try
            {
                int index = 0;
                positions = new Messaging.Protobuf.Positions();

                foreach (cAlgo.API.Position p in _positions)
                {

                    if (!ParseOpenPosition(p, out Messaging.Protobuf.Position position, Messaging.Protobuf.Position.Types.StateChange.None))
                        continue;

                    //Update StateChage if this is a newly opened position
                    if (position.Label == openedPositionLabel)
                    {
                        position.StateChange = Messaging.Protobuf.Position.Types.StateChange.Opened;
                    }

                    positions.Position[index] = position;
                    index++;
                }
                positions.Count = index;
            }
            catch (Exception e)
            {
                Console.WriteLine("FAILED to parse Positions: " + e.ToString());
                positions = null;
                return false;
            }
            return true;
        }

        public static bool ParseOpenPosition(cAlgo.API.Position p, out Messaging.Protobuf.Position position, Messaging.Protobuf.Position.Types.StateChange stateChange = Messaging.Protobuf.Position.Types.StateChange.None)
        {
            try
            {
                position = new Messaging.Protobuf.Position()
                {
                    Id = p.Id,
                    Label = p.Label,
                    Pips = p.Pips,
                    EntryTime = p.EntryTime.ToBinary(),
                    SymbolCode = p.SymbolCode,
                    StopLossPrice = (double)p.StopLoss,
                    TakeProfitPrice = (double)p.TakeProfit,
                    Volume = p.Volume,
                    TradeType = ParsePositionTradeType(p.TradeType),
                    GrossProfit = p.GrossProfit,
                    StateChange = stateChange
                };
            }
            catch (Exception e)
            {
                Console.WriteLine("FAILED to parse Position: " + e.ToString());
                position = null;
                return false;
            }
            return true;
        }

        public static bool ParseClosedPosition(cAlgo.API.Position _position, double closePrice, DateTime closeTime, out Messaging.Protobuf.Position position)
        {
            if (ParseOpenPosition(_position, out position, Messaging.Protobuf.Position.Types.StateChange.Closed))
            {
                position.ClosePrice = closePrice;
                position.CloseTime = closeTime.ToBinary();
                return true;
            }
            return false;
        }
    }
}
