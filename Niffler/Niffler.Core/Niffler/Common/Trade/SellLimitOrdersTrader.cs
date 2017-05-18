using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using cAlgo;
using cAlgo.API;
using Niffler.Common;

namespace Niffler.Common.Trade
{
    class SellLimitOrdersTrader : OrdersManager
    {

        public SellLimitOrdersTrader(State botState, int numberOfOrders, int entryOffSetPips, double defaultTakeProfitPips, double finalOrderStopLossPips) 
            : base(botState,  numberOfOrders,  entryOffSetPips,  defaultTakeProfitPips, finalOrderStopLossPips) { }

        // Place Sell Limit Orders
        public void placeSellLimitOrders()
        {
            //Place Sell Limit Orders
            for (int OrderCount = 0; OrderCount < NumberOfOrders; OrderCount++)
            {
                try
                {
                    TradeData data = new TradeData
                    {
                        tradeType = TradeType.Sell,
                        symbol = Bot.Symbol,
                        volume = setVolume(OrderCount),
                        entryPrice = calcSellEntryPrice(OrderCount),
                        label = BotState.BotId + "-" + Utils.getTimeStamp() + BotState.getMarketName() + "-SWF#" + OrderCount,
                        stopLossPips = setPendingOrderStopLossPips(OrderCount, NumberOfOrders),
                        takeProfitPips = DefaultTakeProfitPips * (1 / Bot.Symbol.TickSize)
                    };
                    if (data == null)
                        continue;

                    //Check that entry price is valid
                    if (data.entryPrice > Bot.Symbol.Ask)
                    {
                        Bot.PlaceLimitOrderAsync(data.tradeType, data.symbol, data.volume, data.entryPrice, data.label, data.stopLossPips, data.takeProfitPips, onTradeOperationComplete);
                    }
                    else
                    {
                        //Tick price has 'jumped' - therefore avoid placing all PendingOrders by re-calculating the OrderCount to the equivelant entry point.
                        OrderCount = calculateNewOrderCount(OrderCount, Bot.Symbol.Ask);
                        Bot.ExecuteMarketOrderAsync(data.tradeType, data.symbol, data.volume, data.label + "X", data.stopLossPips, data.takeProfitPips, onTradeOperationComplete);
                    }
                }
                catch (Exception e)
                {
                    Bot.Print("Failed to place Sell Limit Order: " + e.Message);
                }
            }

            //All Sell Limit Orders have been placed
            BotState.OrdersPlaced = true;
            resetBollingerBand();
        }

        protected double calcSellEntryPrice(int orderCount)
        {

            //OPTIONAL - Bollinger band indicates whether market is oversold or over bought.
            if (useBollingerBandEntry)
            {
                if (EntryBollingerBandPrice == 0)
                {
                    EntryBollingerBandPrice = BollingerBand.Top.Last(0);
                }
                //Use Bolinger Band limit as first order entry point.
                return EntryBollingerBandPrice + calcOrderSpacingDistance(orderCount);
            }
            else
            {
                return BotState.OpenPrice + EntryOffSetPips + calcOrderSpacingDistance(orderCount);
            }
        }
        
    }
}
