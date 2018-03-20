using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Niffler.Messaging.RabbitMQ;
using Niffler.Messaging.Protobuf;
using Niffler.Core.Config;

// HACK: The functionality in this class should be migrated to the TraderService talking to the FIX API 
// together with the TradeManager Service.

namespace Niffler.Core.Trades
{
    public class TradePublisher
    {
        private Publisher Publisher;
        private TradesFactory TradesFactory;
        private String EntityName;
        private TradeUtils TradeUtils;

        public TradePublisher(Publisher publisher, TradeUtils tradeUtils, String entityName)
        {
            this.Publisher = publisher;
            this.EntityName = entityName;
            this.TradeUtils = tradeUtils;
            TradesFactory = new TradesFactory();
        }

        public void PlaceSellLimit(string symbolCode, string label, double volume, double targetEntryPrice)
        {
            PlaceSellLimit(symbolCode, label, volume, targetEntryPrice, -1, -1);
        }

        public void PlaceSellLimit(string symbolCode, string label, double volume, double targetEntryPrice, double TakeProfitPips)
        {
            PlaceSellLimit(symbolCode, label, volume, targetEntryPrice, TakeProfitPips, -1);
        }

        public void PlaceSellLimit(string symbolCode, string label, double volume, double targetEntryPrice, double TakeProfitPips, double StopLossPips)
        {
            PlaceSellStopLossAndTakeProfit(symbolCode, label, volume, targetEntryPrice, TakeProfitPips, StopLossPips);
            Publisher.TradeOperation(TradesFactory.CreateSellLimitTrade(symbolCode, label, volume, targetEntryPrice), EntityName);
        }

        public void PlaceSellStop(string symbolCode, string label, double volume, double targetEntryPrice)
        {
            PlaceSellStop(symbolCode, label, volume, targetEntryPrice, -1, -1);
        }

        public void PlaceSellStop(string symbolCode, string label, double volume, double targetEntryPrice, double TakeProfitPips)
        {
            PlaceSellStop(symbolCode, label, volume, targetEntryPrice, TakeProfitPips, -1);
        }

        public void PlaceSellStop(string symbolCode, string label, double volume, double targetEntryPrice, double TakeProfitPips, double StopLossPips)
        {
            PlaceSellStopLossAndTakeProfit(symbolCode, label, volume, targetEntryPrice, TakeProfitPips, StopLossPips);
            Publisher.TradeOperation(TradesFactory.CreateSellStopTrade(symbolCode, label, volume, targetEntryPrice), EntityName);
        }

        public void PlaceBuyStop(string symbolCode, string label, double volume, double targetEntryPrice)
        {
            PlaceBuyStop(symbolCode, label, volume, targetEntryPrice, -1, -1);
        }

        public void PlaceBuyStop(string symbolCode, string label, double volume, double targetEntryPrice, double TakeProfitPips)
        {
            PlaceBuyStop(symbolCode, label, volume, targetEntryPrice, TakeProfitPips, -1);
        }

        public void PlaceBuyStop(string symbolCode, string label, double volume, double targetEntryPrice, double TakeProfitPips, double StopLossPips)
        {
            PlaceBuyStopLossAndTakeProfit(symbolCode, label, volume, targetEntryPrice, TakeProfitPips, StopLossPips);
            Publisher.TradeOperation(TradesFactory.CreateBuyStopTrade(symbolCode, label, volume, targetEntryPrice), EntityName);
        }

        public void PlaceBuyLimit(string symbolCode, string label, double volume, double targetEntryPrice)
        {
            PlaceBuyLimit(symbolCode, label, volume, targetEntryPrice, -1, -1);
        }

        public void PlaceBuyLimit(string symbolCode, string label, double volume, double targetEntryPrice, double TakeProfitPips)
        {
            PlaceBuyLimit(symbolCode, label, volume, targetEntryPrice, TakeProfitPips, -1);
        }

        public void PlaceBuyLimit(string symbolCode, string label, double volume, double targetEntryPrice, double TakeProfitPips, double StopLossPips)
        {
            PlaceBuyStopLossAndTakeProfit(symbolCode, label, volume, targetEntryPrice, TakeProfitPips, StopLossPips);
            Publisher.TradeOperation(TradesFactory.CreateBuyLimitTrade(symbolCode, label, volume, targetEntryPrice), EntityName);
        }

        public void PlaceBuy(string symbolCode, string label, double volume, double targetEntryPrice, double TakeProfitPips, double StopLossPips)
        {
            PlaceBuyStopLossAndTakeProfit(symbolCode, label, volume, targetEntryPrice, TakeProfitPips, StopLossPips);
            Publisher.TradeOperation(TradesFactory.CreateBuyTrade(symbolCode, label, volume, targetEntryPrice), EntityName);
        }

        public void PlaceSell(string symbolCode, string label, double volume, double targetEntryPrice, double TakeProfitPips, double StopLossPips)
        {
            PlaceSellStopLossAndTakeProfit(symbolCode, label, volume, targetEntryPrice, TakeProfitPips, StopLossPips);
            Publisher.TradeOperation(TradesFactory.CreateSellTrade(symbolCode, label, volume, targetEntryPrice), EntityName);
        }


        private void PlaceBuyStopLossAndTakeProfit(string symbolCode, string label, double volume, double targetEntryPrice, double TakeProfitPips, double StopLossPips)
        {
            //Notify TradeManager to place SL Order
            if (StopLossPips > 0)
            {
                PlaceBuyStopLoss(symbolCode, label, volume, targetEntryPrice, TakeProfitPips, StopLossPips);
            }

            //Notify TradeManager to place TP Order
            if (TakeProfitPips > 0)
            {
                PlaceBuyTakeProfit(symbolCode, label, volume, targetEntryPrice, TakeProfitPips, StopLossPips);
            }
        }

        private void PlaceSellStopLossAndTakeProfit(string symbolCode, string label, double volume, double targetEntryPrice, double TakeProfitPips, double StopLossPips)
        {
            //Notify TradeManager to place SL Order
            if (StopLossPips > 0)
            {
                PlaceSellStopLoss(symbolCode, label, volume, targetEntryPrice, TakeProfitPips, StopLossPips);
            }

            //Notify TradeManager to place TP Order
            if (TakeProfitPips > 0)
            {
                PlaceSellTakeProfit(symbolCode, label, volume, targetEntryPrice, TakeProfitPips, StopLossPips);
            }
        }

        private void PlaceSellStopLoss(string symbolCode, string label, double volume, double targetEntryPrice, double TakeProfitPips, double StopLossPips)
        {
            //Publish message to TradeManager to Place StopLoss BuyStopOrder when SellStopOrder filled
            Publisher.TradeManagement(TradesFactory.CreateBuyStopTrade(symbolCode, label + "-SL", volume, targetEntryPrice + TradeUtils.CalcPipsForBroker(StopLossPips)), label, EntityName);
        }

        private void PlaceSellTakeProfit(string symbolCode, string label, double volume, double targetEntryPrice, double TakeProfitPips, double StopLossPips)
        {
            //Publish message to TradeManager to Place TakeProfit BuyLimitOrder when SellStopOrder filled
            Publisher.TradeManagement(TradesFactory.CreateBuyLimitTrade(symbolCode, label + "-TP", volume, targetEntryPrice - TradeUtils.CalcPipsForBroker(TakeProfitPips)), label, EntityName);
        }

        private void PlaceBuyStopLoss(string symbolCode, string label, double volume, double targetEntryPrice, double TakeProfitPips, double StopLossPips)
        {
            //Publish message to TradeManager to Place StopLoss SellStopOrder when BuyLimitOrder filled
            Publisher.TradeManagement(TradesFactory.CreateSellStopTrade(symbolCode, label + "-SL", volume, targetEntryPrice - TradeUtils.CalcPipsForBroker(StopLossPips)), label, EntityName);
        }

        private void PlaceBuyTakeProfit(string symbolCode, string label, double volume, double targetEntryPrice, double TakeProfitPips, double StopLossPips)
        {
            //Publish message to TradeManager to Place TakeProfit SellLimitOrder when BuyLimitOrder filled
            Publisher.TradeManagement(TradesFactory.CreateSellLimitTrade(symbolCode, label + "-TP", volume, targetEntryPrice + TradeUtils.CalcPipsForBroker(TakeProfitPips)), label, EntityName);
        }




    }
}
