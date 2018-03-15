using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Niffler.Messaging.RabbitMQ;
using Niffler.Messaging.Protobuf;

// HACK - The functionality in this class should be migrated to the TraderService talking to the FIX API 
// together with the TradeManager Service.

namespace Niffler.Core.Trades
{
    public class TradePublisher
    {
        private Publisher Publisher;
        private TradesFactory TradesFactory;
        private String EntityName;

        public TradePublisher(Publisher publisher, String entityName)
        {
            this.Publisher = publisher;
            TradesFactory = new TradesFactory();
            this.EntityName = entityName;
        }

        public Trade PlaceSellLimitOrder(string symbolCode, string label, int volume, double targetEntryPrice)
        {
            return PlaceSellLimitOrder(symbolCode, label, volume, targetEntryPrice, -1, -1);
        }

        public Trade PlaceSellLimitOrder(string symbolCode, string label, int volume, double targetEntryPrice, double TakeProfitPips)
        {
            return PlaceSellLimitOrder(symbolCode, label, volume, targetEntryPrice, TakeProfitPips, -1);
        }

        public Trade PlaceSellLimitOrder(string symbolCode, string label, int volume, double targetEntryPrice, double TakeProfitPips, double StopLossPips)
        {
            
            //Notify TradeManager to place SL Order
            if (StopLossPips > 0)
            {
                //Publish message to TradeManager to Place StopLoss BuyStopOrder when SellLimitOrder filled
                Publisher.TradeManagement(TradesFactory.CreateBuyStopOrderTrade(symbolCode,label+"-SL",volume,targetEntryPrice), label,StopLossPips);
            }

            //Notify TradeManager to place TP Order
            if (TakeProfitPips > 0)
            {
                //Publish message to TradeManager to Place TakeProfit SellStopOrder when SellLimitOrder filled
                Publisher.TradeManagement(PlaceSellStopTrade);
            }

            //Create SellLimitOrder
            Trade SellLimitOrderTrade = TradesFactory.CreateSellLimitOrderTrade(symbolCode, label, volume, targetEntryPrice);
            Publisher.TradeOperation(SellLimitOrderTrade, EntityName);
            return SellLimitOrderTrade;
        }

        public void PlaceSellStopOrder(string symbolCode, string label, int volume, double targetEntryPrice, double TakeProfitPips, double StopLossPips)
        {
            CreateSellStopTrade(symbolCode, label, volume, targetEntryPrice);

            //Place SL Order
            if (StopLossPips > 0)
            {
                TradeManager.setSellStopLoss(label, StopLossPips);
            }

            //Place TP Order
            if (TakeProfitPips > 0)
            {
                TradeManager.setSellTakeProfit(label, TakeProfitPips);
            }
        }

        public void CreateBuyOrder(string symbolCode, string label, int volume, double targetEntryPrice, double TakeProfitPips, double StopLossPips)
        {
            CreateBuyOrderTrade(symbolCode, label, volume, targetEntryPrice);

            //Place SL Order
            if (StopLossPips > 0)
            {
                TradeManager.setBuyStopLoss(label, StopLossPips);
            }

            //Place TP Order
            if (TakeProfitPips > 0)
            {
                TradeManager.setBuyTakeProfit(label, TakeProfitPips);
            }
        }
        
    }
}
