using System;
using cAlgo.API;

namespace Niffler.Common.Trade
{
    class SellLimitOrdersTrader : OrdersManager
    {

        public SellLimitOrdersTrader(StateData botState, int numberOfOrders, int entryTriggerOrderPlacementPips, int entryOffSetPips, double defaultTakeProfitPips, double finalOrderStopLossPips) 
            : base(botState,  numberOfOrders, entryTriggerOrderPlacementPips, entryOffSetPips,  defaultTakeProfitPips, finalOrderStopLossPips) { }

        // Place Sell Limit Orders
        public void PlaceSellLimitOrders()
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
                        volume = SetVolume(OrderCount),
                        entryPrice = CalcSellEntryPrice(OrderCount),
                        label = BotState.BotId + "-" + Utils.GetTimeStamp(Bot) + BotState.GetMarketName() + "-SWF#" + OrderCount,
                        stopLossPips = SetPendingOrderStopLossPips(OrderCount, NumberOfOrders),
                        takeProfitPips = DefaultTakeProfitPips * (1 / Bot.Symbol.TickSize)
                    };
                    if (data == null)
                        continue;

                    //Check that entry price is valid
                    if (data.entryPrice > Bot.Symbol.Ask)
                    {
                        Bot.PlaceLimitOrderAsync(data.tradeType, data.symbol, data.volume, data.entryPrice, data.label, data.stopLossPips, data.takeProfitPips, OnPlaceSellLimitOrderOperationComplete);
                    }
                    else
                    {
                        //Tick price has 'jumped' - therefore avoid placing all PendingOrders by re-calculating the OrderCount to the equivelant entry point.
                        OrderCount = CalcNewOrderCount(OrderCount, Bot.Symbol.Ask);
                        Bot.ExecuteMarketOrderAsync(data.tradeType, data.symbol, data.volume, data.label + "X", data.stopLossPips, data.takeProfitPips, OnPlaceSellOperationComplete);
                    }
                }
                catch (Exception e)
                {
                    Bot.Print("Failed to place Sell Limit Order: " + e.Message);
                }
            }
        }

        protected double CalcSellEntryPrice(int orderCount)
        {

            //OPTIONAL - Bollinger band indicates whether market is oversold or over bought.
            if (useBollingerBandEntry)
            {
                if (EntryBollingerBandPrice == 0)
                {
                    EntryBollingerBandPrice = BollingerBand.Top.Last(0);
                }
                //Use Bolinger Band limit as first order entry point.
                return EntryBollingerBandPrice + CalcOrderSpacingDistance(orderCount);
            }
            else
            {
                return BotState.OpenPrice + EntryOffSetPips + CalcOrderSpacingDistance(orderCount);
            }
        }

        protected void OnPlaceSellLimitOrderOperationComplete(TradeResult tr)
        {
            OnPendingOrderOperationComplete(tr, "FAILED to place Limit Order");
        }

        protected void OnPlaceSellOperationComplete(TradeResult tr)
        {
            OnPositionOperationComplete(tr, "FAILED to place position");
        }
        


    }
}
