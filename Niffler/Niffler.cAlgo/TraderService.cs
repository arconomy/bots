using cAlgo.API;
using Niffler.Messaging.RabbitMQ;
using Niffler.Messaging.Protobuf;
using System;
using System.Threading;

namespace Niffler.cAlgoClient

{
    public class TraderService : cAlgo.API.Robot
    {
        private TestTradeManager TestTradeManager;

        protected override void OnStart()
        {
            //Set up Adapter to manage single connection for all consumers.
            Adapter Adapter = Adapter.Instance;
            Adapter.Init();
            Adapter.Connect();

            TestTradeManager = new TestTradeManager(MarketSeries.SymbolCode);
            TestTradeManager.Run(Adapter);
           
            Positions.Opened += OnPositionOpened;
            Positions.Closed += OnPositionClosed;

            TestTradeManager.ExecuteBuyMarketOrder = ExecuteBuyMarketOrder;
            TestTradeManager.ExecuteSellMarketOrder = ExecuteSellMarketOrder;
            TestTradeManager.PlaceBuyLimitOrder = PlaceBuyLimitOrder;
            TestTradeManager.PlaceBuyStopOrder = PlaceBuyStopOrder;
            TestTradeManager.PlaceSellLimitOrder = PlaceSellLimitOrder;
            TestTradeManager.PlaceSellStopOrder = PlaceSellStopOrder;
            TestTradeManager.ModifyPosition = ModifyPosition;
            TestTradeManager.ClosePosition = CloseTradePosition;
            TestTradeManager.CancelOrder = CancelOrder;
            TestTradeManager.ModifyOrder = ModifyOrder;
        }

        protected override void OnTick()
        {
            //Send Tick, Positions and Pending Orders
            TestTradeManager.PublishOnTickEvent(Symbol, Positions, PendingOrders, Server.Time, this.IsBacktesting);
        }

        protected void OnPositionOpened(PositionOpenedEventArgs args)
        {
            //Send Opened Position, Positions and Pending Orders
            TestTradeManager.PublishOnPositionOpened(args.Position, Positions, PendingOrders, this.IsBacktesting);
        }

        protected void OnPositionClosed(PositionClosedEventArgs args)
        {
            //Send Closed Position, Positions and Pending Orders
            double closePrice = this.History.FindLast(args.Position.Label, Symbol, args.Position.TradeType).ClosingPrice;
            TestTradeManager.PublishOnPositionClosed(args.Position, closePrice, Positions, PendingOrders, Server.Time, this.IsBacktesting);
        }

        protected void ExecuteBuyMarketOrder(Trade trade)
        {
            this.ExecuteMarketOrderAsync(TradeType.Buy, Symbol, (long)trade.Position.Volume, ExcuteMarketOrderCallBack);
        }

        protected void ExecuteSellMarketOrder(Trade trade)
        {
            this.ExecuteMarketOrderAsync(TradeType.Sell, Symbol, (long)trade.Position.Volume, ExcuteMarketOrderCallBack);
        }

        private void ExcuteMarketOrderCallBack(TradeResult obj)
        {
            throw new NotImplementedException();
        }

        protected void PlaceBuyLimitOrder(Trade trade)
        {
            this.PlaceLimitOrderAsync(TradeType.Buy, Symbol, (long)trade.Order.Volume, trade.Order.TargetEntryPrice, trade.Order.Label,trade.Order.StopLossPips,trade.Order.TakeProfitPips, PlaceBuyLimitOrderCallBack);
        }

        private void PlaceBuyLimitOrderCallBack(TradeResult obj)
        {
            throw new NotImplementedException();
        }

        protected void PlaceBuyStopOrder(Trade trade)
        {
            this.PlaceStopOrderAsync(TradeType.Buy, Symbol, (long)trade.Order.Volume, trade.Order.TargetEntryPrice, trade.Order.Label, trade.Order.StopLossPips, trade.Order.TakeProfitPips, PlaceBuyStopOrderCallBack);
        }

        private void PlaceBuyStopOrderCallBack(TradeResult obj)
        {
            throw new NotImplementedException();
        }

        protected void PlaceSellLimitOrder(Trade trade)
        {
            this.PlaceLimitOrderAsync(TradeType.Sell, Symbol, (long) trade.Order.Volume, trade.Order.TargetEntryPrice, trade.Order.Label, trade.Order.StopLossPips, trade.Order.TakeProfitPips, PlaceSellLimitOrderCallBack);
        }

        private void PlaceSellLimitOrderCallBack(TradeResult obj)
        {
            throw new NotImplementedException();
        }

        protected void PlaceSellStopOrder(Trade trade)
        {
            this.PlaceLimitOrderAsync(TradeType.Sell, Symbol, (long) trade.Order.Volume, trade.Order.TargetEntryPrice, trade.Order.Label, trade.Order.StopLossPips, trade.Order.TakeProfitPips, PlaceSellStopOrderCallBack);
        }

        private void PlaceSellStopOrderCallBack(TradeResult obj)
        {
            throw new NotImplementedException();
        }

        protected void ModifyPosition(Trade trade)
        {
            this.ModifyPositionAsync(Positions.Find(trade.Order.Label), trade.Order.StopLossPips, trade.Order.TakeProfitPips, ModifyPositionCallBack);
        }

        private void ModifyPositionCallBack(TradeResult obj)
        {
            throw new NotImplementedException();
        }

        protected void CloseTradePosition(Trade trade)
        {
            this.ClosePositionAsync(Positions.Find(trade.Position.Label), ClosePositionCallBack);
        }

        private void ClosePositionCallBack(TradeResult obj)
        {
            throw new NotImplementedException();
        }

        protected void CancelOrder(Trade trade)
        {
            if (FindPendingOrder(PendingOrders, trade.Order.Label, out PendingOrder order))
                CancelPendingOrderAsync(order, CancelOrderCallBack);
        }

        private void CancelOrderCallBack(TradeResult obj)
        {
            throw new NotImplementedException();
        }

        protected void ModifyOrder(Trade trade)
        {
            if (FindPendingOrder(PendingOrders, trade.Order.Label, out PendingOrder order))
                this.ModifyPendingOrderAsync(order,trade.Order.TargetEntryPrice, trade.Order.StopLossPips,trade.Order.TakeProfitPips,null, ModifyOrderCallBack);
        }

        private void ModifyOrderCallBack(TradeResult obj)
        {
            throw new NotImplementedException();
        }

        protected override void OnStop()
        {
            TestTradeManager.ShutDown();
            Thread.Sleep(50);
        }

        private bool FindPendingOrder(PendingOrders pendingOrders, string label, out PendingOrder order)
        {
            foreach (PendingOrder p in pendingOrders)
            {
                if (p.Label == label)
                {
                    order = p;
                    return true;
                }
            }
            order = null;
            return false;
        }
    }
}
























