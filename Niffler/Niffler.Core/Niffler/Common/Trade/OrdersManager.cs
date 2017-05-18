using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using cAlgo;
using cAlgo.API;
using cAlgo.API.Indicators;

namespace Niffler.Common.Trade
{
    abstract class OrdersManager
    {
        protected State BotState { get; set; }
        protected Robot Bot { get; set; }
        protected bool useBollingerBandEntry;

        protected double EntryBollingerBandPrice;
        protected int NumberOfOrders;
        protected int EntryOffSetPips;
        protected double DefaultTakeProfitPips;
        protected double FinalOrderStopLossPips;

        protected int MultiplyOrderSpacingEveryNthOrder;
        protected double OrderSpacingMultipler;
        protected int OrderSpacingMaxPips;
        protected int OrderSpacingPips;
        private bool UseVariableOrderSpacing;

        protected int MultiplyVolumeEveryNthOrder;
        protected double VolumeMultipler;
        protected int MaxVolumeLots;
        protected int BaseVolumeLots;
        protected bool UseVariableVolume;

        protected BollingerBands BollingerBand;


        public OrdersManager(State s, int numberOfOrders, int entryOffSetPips, double defaultTakeProfitPips, double finalOrderStopLossPips)
        { 
            BotState = s;
            Bot = BotState.Bot;
            NumberOfOrders = numberOfOrders;
            EntryOffSetPips = entryOffSetPips;
            DefaultTakeProfitPips = defaultTakeProfitPips;
            FinalOrderStopLossPips = finalOrderStopLossPips;
        }

        public void setVolumeMultipler(int multiplyVolumeEveryNthOrder, double volumeMultipler, int maxVolumeLots, int baseVolumeLots)
        {
            MultiplyVolumeEveryNthOrder = multiplyVolumeEveryNthOrder;
            VolumeMultipler = volumeMultipler;
            MaxVolumeLots = maxVolumeLots;
            BaseVolumeLots = baseVolumeLots;
            UseVariableVolume = true;
        }

        public void setOrderSpacing(int multiplyOrderSpacingEveryNthOrder, double orderSpacingMultipler, int orderSpacingMaxPips, int orderSpacingPips)
        {
            MultiplyOrderSpacingEveryNthOrder = multiplyOrderSpacingEveryNthOrder;
            OrderSpacingMultipler = orderSpacingMultipler;
            OrderSpacingMaxPips = orderSpacingMaxPips;
            OrderSpacingPips = orderSpacingPips;
            UseVariableOrderSpacing = true;
        }

        protected void resetBollingerBand()
        {
            EntryBollingerBandPrice = 0;
        }

        public void setBollingerBandDefault(DataSeries source)
        {
            BollingerBand = Bot.Indicators.BollingerBands(source, 2, 20, MovingAverageType.Exponential);
            useBollingerBandEntry = true;
        }

        public void setBollingerBand(int insideBollingerEntryPips, DataSeries source, int periods, double standDeviations, MovingAverageType maType)
        {
            BollingerBand = Bot.Indicators.BollingerBands(source, periods, standDeviations, maType);
            useBollingerBandEntry = true;
        }

        //Calculate a new orderCount number for when tick jumps
        protected int calculateNewOrderCount(int orderCount, double currentTickPrice)
        {
            double tickJumpIntoRange = Math.Abs(BotState.OpenPrice - currentTickPrice) - EntryOffSetPips;
            double pendingOrderRange = calcOrderSpacingDistance(NumberOfOrders);
            double pendingOrdersPercentageJumped = tickJumpIntoRange / pendingOrderRange;
            double newOrderCount = NumberOfOrders * pendingOrdersPercentageJumped;

            if (newOrderCount > orderCount)
                return (int)newOrderCount;
            else
                return (int)orderCount;
        }

        //Returns the entry distance from the first entry point to an order based on MultiplyOrderSpacingEveryNthOrder, OrderSpacingMultipler and OrderSpacingPips until OrderSpacingMax reached
        protected int calcOrderSpacingDistance(int orderCount)
        {
            if(!UseVariableOrderSpacing)
            {
                return OrderSpacingPips;
            }

            double orderSpacingLevel = 0;
            double orderSpacing = 0;
            double orderSpacingResult = 0;

            for (int i = 1; i <= orderCount; i++)
            {
                orderSpacingLevel = i / MultiplyOrderSpacingEveryNthOrder;
                orderSpacing = Math.Pow(OrderSpacingMultipler, orderSpacingLevel) * OrderSpacingPips;

                if (orderSpacing > OrderSpacingMaxPips)
                {
                    orderSpacing = OrderSpacingMaxPips;
                }

                orderSpacingResult += orderSpacing;
            }

            return (int)orderSpacingResult;
        }

        //Set a stop loss on the last Pending Order set to catch the break away train that never comes back!
        protected double setPendingOrderStopLossPips(int orderCount, int numberOfOrders)
        {
            if (orderCount == numberOfOrders - 1)
            {
                return FinalOrderStopLossPips * (1 / Bot.Symbol.TickSize);
            }
            else
            {
                return 0;
            }
        }

        //Increase the volume based on Orders places and volume levels and multiplier until max volume reached
        protected int setVolume(int orderCount)
        {
            if(!UseVariableVolume)
            {
                return BaseVolumeLots;
            }

            double orderVolumeLevel = orderCount / MultiplyVolumeEveryNthOrder;
            double volume = Math.Pow(VolumeMultipler, orderVolumeLevel) * BaseVolumeLots;

            if (volume > MaxVolumeLots)
            {
                volume = MaxVolumeLots;
            }

            return (int)volume;
        }

        public void closeAllPendingOrders()
        {
            //Close any outstanding pending orders
            foreach (PendingOrder po in Bot.PendingOrders)
            {
                try
                {
                    if (BotState.isThisBotId(po.Label))
                    {
                        Bot.CancelPendingOrderAsync(po, onTradeOperationComplete);
                    }
                }
                catch (Exception e)
                {
                    Bot.Print("Failed to Cancel Pending Order :" + e.Message);
                }
            }
            BotState.IsPendingOrdersClosed = true;
        }

        protected void onTradeOperationComplete(TradeResult tr)
        {
            if (!tr.IsSuccessful)
            {
                string msg = "FAILED Trade Operation for Order: " + tr.Error;
                Bot.Print(msg, " Pending Order: ", tr.PendingOrder.Label, " ", tr.PendingOrder.TradeType, " ", System.DateTime.Now);
            }
        }
    }
}
