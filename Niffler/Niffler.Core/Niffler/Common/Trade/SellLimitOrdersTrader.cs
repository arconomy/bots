using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using cAlgo;
using cAlgo.API;

namespace Niffler.Common.Trade
{
    class SellLimitOrdersTrader
    {

        private State BotState { get; set; }
        private Robot Bot { get; set; }

        public SellLimitOrdersTrader(Robot r, State s)
        {
            Bot = r;
            BotState = s;
        }

        // Place Sell Limit Orders
        protected void placeSellLimitOrders(int numberOfOrders)
        {
            //Place Sell Limit Orders
            for (int OrderCount = 0; OrderCount < numberOfOrders; OrderCount++)
            {
                try
                {
                    TradeData data = new TradeData
                    {
                        tradeType = TradeType.Sell,
                        symbol = Symbol,
                        volume = setVolume(OrderCount),
                        entryPrice = calcSellEntryPrice(OrderCount),
                        label = _botId + "-" + getTimeStamp() + _swordFishTimeInfo.market + "-SWF#" + OrderCount,
                        stopLossPips = setPendingOrderStopLossPips(OrderCount, NumberOfOrders),
                        takeProfitPips = TakeProfit * (1 / Symbol.TickSize)
                    };
                    if (data == null)
                        continue;

                    //Check that entry price is valid
                    if (data.entryPrice > Symbol.Ask)
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
        }

        protected double calcSellEntryPrice(int orderCount)
        {

            //OPTIONAL - Bollinger band indicates whether market is oversold or over bought.
            if (useBollingerBandEntry)
            {
                //Use Bolinger Band limit as first order entry point.
                return _boli.Top.Last(0) - targetBolliEntryPips + calcOrderSpacingDistance(orderCount);
            }
            else
            {
                return BotState.OpenPrice + BotState.OrderEntryOffset + calcOrderSpacingDistance(orderCount);
            }
        }

        protected void onTradeOperationComplete(TradeResult tr)
        {
            if (!tr.IsSuccessful)
            {
                string msg = "FAILED Trade Operation: " + tr.Error;
                if (tr.Position != null)
                    Print(msg, " Position: ", tr.Position.Label, " ", tr.Position.TradeType, " ", Time);
                if (tr.PendingOrder != null)
                    Print(msg, " Pending Order: ", tr.PendingOrder.Label, " ", tr.PendingOrder.TradeType, " ", Time);
            }
        }

    }
}
