using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using cAlgo;
using cAlgo.API;

namespace Niffler.Common.Trade
{
    class BuyLimitOrdersTrader : OrdersManager
    {
        public BuyLimitOrdersTrader(State botState, int numberOfOrders, int entryTriggerOrderPlacementPips, int entryOffSetPips, double defaultTakeProfitPips, double finalOrderStopLossPips) 
            : base(botState,  numberOfOrders, entryTriggerOrderPlacementPips, entryOffSetPips,  defaultTakeProfitPips, finalOrderStopLossPips) { }

        //Place Buy Limit Orders
        public void PlaceBuyLimitOrders()
        {
            //Place Buy Limit Orders
            for (int OrderCount = 0; OrderCount < NumberOfOrders; OrderCount++)
            {
                try
                {
                    TradeData data = new TradeData
                    {
                        tradeType = TradeType.Buy,
                        symbol = Bot.Symbol,
                        volume = SetVolume(OrderCount),
                        entryPrice = CalcBuyEntryPrice(OrderCount),
                        label = BotState.BotId + "-" + Utils.GetTimeStamp() + BotState.GetMarketName() + "-SWF#" + OrderCount,
                        stopLossPips = SetPendingOrderStopLossPips(OrderCount, NumberOfOrders),
                        takeProfitPips = DefaultTakeProfitPips * (1 / Bot.Symbol.TickSize)
                    };

                    if (data == null)
                        continue;

                    //Check that entry price is valid
                    if (data.entryPrice < Bot.Symbol.Bid)
                    {
                        Bot.PlaceLimitOrderAsync(data.tradeType, data.symbol, data.volume, data.entryPrice, data.label, data.stopLossPips, data.takeProfitPips, OnPlaceBuyLimitOrderOperationComplete);
                    }
                    else
                    {
                        //Tick price has 'jumped' - therefore avoid placing all PendingOrders by re-calculating the OrderCount to the equivelant entry point.
                        OrderCount = CalcNewOrderCount(OrderCount, Bot.Symbol.Bid);
                        Bot.ExecuteMarketOrderAsync(data.tradeType, data.symbol, data.volume, data.label + "X", data.stopLossPips, data.takeProfitPips, OnBuyTradeOperationComplete);
                    }
                }
                catch (Exception e)
                {
                    Bot.Print("Failed to place buy limit order: " + e.Message);
                }
            }

            //All Buy Limit Orders have been placed
            BotState.OrdersPlaced = true;
            ResetBollingerBand();
        }

        protected double CalcBuyEntryPrice(int orderCount)
        {
            //OPTIONAL - Bollinger band indicates whether market is oversold or over bought.
            if (useBollingerBandEntry)
            {
                if(EntryBollingerBandPrice == 0)
                {
                    EntryBollingerBandPrice = BollingerBand.Bottom.Last(0);
                }
                //Use Bolinger Band limit as first order entry point.
                return EntryBollingerBandPrice - CalcOrderSpacingDistance(orderCount);
            }
            else
            {
                return BotState.OpenPrice - EntryOffSetPips - CalcOrderSpacingDistance(orderCount);
            }
        }

        protected void OnPlaceBuyLimitOrderOperationComplete(TradeResult tr)
        {
            OnPendingOrderOperationComplete(tr, "FAILED to place BUY Limit Order");
        }

        protected void OnBuyTradeOperationComplete(TradeResult tr)
        {
            OnPositionOperationComplete(tr, "FAILED to place BUY position");
        }



    }
}
