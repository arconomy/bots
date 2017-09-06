using cAlgo.API;
using Niffler.Services;
using Niffler.Messaging.RabbitMQ;
using Niffler.Messaging.Protobuf;
using System;
using Niffler.Common;

namespace Niffler.cAlgoClient

{
    public class TraderService : cAlgo.API.Robot
    {
        private TradeManager TradeManager;

        protected override void OnStart()
        {
            //Set up Adapter to manage single connection for all consumers.
            Adapter Adapter = Adapter.Instance;
            Adapter.Init();
            Adapter.Connect();

            TradeManager = new TradeManager(MarketSeries.SymbolCode);
            TradeManager.Run(Adapter);
           
            Positions.Opened += OnPositionOpened;
            Positions.Closed += OnPositionClosed;

            TradeManager.ExecuteBuyMarketOrder = ExecuteBuyMarketOrder;
            TradeManager.ExecuteSellMarketOrder = ExecuteSellMarketOrder;
            TradeManager.PlaceBuyLimitOrder = PlaceBuyLimitOrder;
            TradeManager.PlaceBuyStopOrder = PlaceBuyStopOrder;
            TradeManager.PlaceSellLimitOrder = PlaceSellLimitOrder;
            TradeManager.PlaceSellStopOrder = PlaceSellStopOrder;
            TradeManager.ModifyPosition = ModifyPosition;
            TradeManager.ClosePosition = CloseTradePosition;
            TradeManager.CancelOrder = CancelOrder;
            TradeManager.ModifyOrder = ModifyOrder;
        }

        protected override void OnTick()
        {
            //Send Tick, Positions and Pending Orders
            TradeManager.PublishOnTickEvent(Symbol, Positions, PendingOrders, Server.Time, this.IsBacktesting);
        }

        protected void OnPositionOpened(PositionOpenedEventArgs args)
        {
            //Send Opened Position, Positions and Pending Orders
            TradeManager.PublishOnPositionOpened(args.Position, Positions, PendingOrders, this.IsBacktesting);
        }

        protected void OnPositionClosed(PositionClosedEventArgs args)
        {
            //Send Closed Position, Positions and Pending Orders
            double closePrice = this.History.FindLast(args.Position.Label, Symbol, args.Position.TradeType).ClosingPrice;
            TradeManager.PublishOnPositionClosed(args.Position, closePrice, Positions, PendingOrders, Server.Time, this.IsBacktesting);
        }

        protected void ExecuteBuyMarketOrder(Trade trade)
        {
            this.ExecuteMarketOrderAsync(TradeType.Buy, Symbol, trade.Position.Volume, ExcuteMarketOrderCallBack);
        }

        protected void ExecuteSellMarketOrder(Trade trade)
        {
            this.ExecuteMarketOrderAsync(TradeType.Sell, Symbol, trade.Position.Volume, ExcuteMarketOrderCallBack);
        }

        private void ExcuteMarketOrderCallBack(TradeResult obj)
        {
            throw new NotImplementedException();
        }

        protected void PlaceBuyLimitOrder(Trade trade)
        {
            this.PlaceLimitOrderAsync(TradeType.Buy, Symbol, trade.Order.Volume, trade.Order.TargetEntryPrice, trade.Order.Label,trade.Order.StopLossPips,trade.Order.TakeProfitPips, PlaceBuyLimitOrderCallBack);
        }

        private void PlaceBuyLimitOrderCallBack(TradeResult obj)
        {
            throw new NotImplementedException();
        }

        protected void PlaceBuyStopOrder(Trade trade)
        {
            this.PlaceStopOrderAsync(TradeType.Buy, Symbol, trade.Order.Volume, trade.Order.TargetEntryPrice, trade.Order.Label, trade.Order.StopLossPips, trade.Order.TakeProfitPips, PlaceBuyStopOrderCallBack);
        }

        private void PlaceBuyStopOrderCallBack(TradeResult obj)
        {
            throw new NotImplementedException();
        }

        protected void PlaceSellLimitOrder(Trade trade)
        {
            this.PlaceLimitOrderAsync(TradeType.Sell, Symbol, trade.Order.Volume, trade.Order.TargetEntryPrice, trade.Order.Label, trade.Order.StopLossPips, trade.Order.TakeProfitPips, PlaceSellLimitOrderCallBack);
        }

        private void PlaceSellLimitOrderCallBack(TradeResult obj)
        {
            throw new NotImplementedException();
        }

        protected void PlaceSellStopOrder(Trade trade)
        {
            this.PlaceLimitOrderAsync(TradeType.Sell, Symbol, trade.Order.Volume, trade.Order.TargetEntryPrice, trade.Order.Label, trade.Order.StopLossPips, trade.Order.TakeProfitPips, PlaceSellStopOrderCallBack);
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
            if (Utils.FindPendingOrder(PendingOrders, trade.Order.Label, out PendingOrder order))
                CancelPendingOrderAsync(order, CancelOrderCallBack);
        }

        private void CancelOrderCallBack(TradeResult obj)
        {
            throw new NotImplementedException();
        }

        protected void ModifyOrder(Trade trade)
        {
            if (Utils.FindPendingOrder(PendingOrders, trade.Order.Label, out PendingOrder order))
                this.ModifyPendingOrderAsync(order,trade.Order.TargetEntryPrice, trade.Order.StopLossPips,trade.Order.TakeProfitPips,null, ModifyOrderCallBack);
        }

        private void ModifyOrderCallBack(TradeResult obj)
        {
            throw new NotImplementedException();
        }

        protected override void OnStop()
        {
            TradeManager.ShutDown();
        }
    }
}
























