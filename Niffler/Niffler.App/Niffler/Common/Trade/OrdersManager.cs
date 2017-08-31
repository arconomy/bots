using System;
using cAlgo.API;
using cAlgo.API.Indicators;

namespace Niffler.Common.Trade
{
    abstract class OrdersManager
    {
        protected StateData BotState { get; set; }
        protected Robot Bot { get; set; }
        protected ReportManager Reporter { get; set; }
        protected bool useBollingerBandEntry;

        protected double EntryBollingerBandPrice;
        protected int NumberOfOrders;
        protected int EntryOffSetPips;
        public int EntryTriggerOrderPlacementPips { get; }
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


        public OrdersManager(StateData s, int numberOfOrders, int entryTriggerOrderPlacementPips, int entryOffSetPips, double defaultTakeProfitPips, double finalOrderStopLossPips)
        { 
            BotState = s;
            Bot = BotState.Bot;
            Reporter = BotState.GetReporter();
            NumberOfOrders = numberOfOrders;
            EntryOffSetPips = entryOffSetPips;
            DefaultTakeProfitPips = defaultTakeProfitPips;
            FinalOrderStopLossPips = finalOrderStopLossPips;
            EntryTriggerOrderPlacementPips = entryTriggerOrderPlacementPips;
        }

        public void SetVolumeMultipler(int multiplyVolumeEveryNthOrder, double volumeMultipler, int maxVolumeLots, int baseVolumeLots)
        {
            MultiplyVolumeEveryNthOrder = multiplyVolumeEveryNthOrder;
            VolumeMultipler = volumeMultipler;
            MaxVolumeLots = maxVolumeLots;
            BaseVolumeLots = baseVolumeLots;
            UseVariableVolume = true;
        }

        public void SetOrderSpacing(int multiplyOrderSpacingEveryNthOrder, double orderSpacingMultipler, int orderSpacingMaxPips, int orderSpacingPips)
        {
            MultiplyOrderSpacingEveryNthOrder = multiplyOrderSpacingEveryNthOrder;
            OrderSpacingMultipler = orderSpacingMultipler;
            OrderSpacingMaxPips = orderSpacingMaxPips;
            OrderSpacingPips = orderSpacingPips;
            UseVariableOrderSpacing = true;
        }

        public void ResetBollingerBand()
        {
            EntryBollingerBandPrice = 0;
        }

        public void SetBollingerBandDefault(DataSeries source)
        {
            BollingerBand = Bot.Indicators.BollingerBands(source, 2, 20, MovingAverageType.Exponential);
            useBollingerBandEntry = true;
        }

        public void SetBollingerBand(int insideBollingerEntryPips, DataSeries source, int periods, double standDeviations, MovingAverageType maType)
        {
            BollingerBand = Bot.Indicators.BollingerBands(source, periods, standDeviations, maType);
            useBollingerBandEntry = true;
        }

        public bool IsOutSideBollingerBand()
        {
            //TO DO: Implement bollinger band in new class with own Rules
            return true;
        }



        //Calculate a new orderCount number for when tick jumps
        protected int CalcNewOrderCount(int orderCount, double currentTickPrice)
        {
            double tickJumpIntoRange = Math.Abs(BotState.OpenPrice - currentTickPrice) - EntryOffSetPips;
            double pendingOrderRange = CalcOrderSpacingDistance(NumberOfOrders);
            double pendingOrdersPercentageJumped = tickJumpIntoRange / pendingOrderRange;
            double newOrderCount = NumberOfOrders * pendingOrdersPercentageJumped;

            if (newOrderCount > orderCount)
                return (int)newOrderCount;
            else
                return (int)orderCount;
        }

        //Returns the entry distance from the first entry point to an order based on MultiplyOrderSpacingEveryNthOrder, OrderSpacingMultipler and OrderSpacingPips until OrderSpacingMax reached
        protected int CalcOrderSpacingDistance(int orderCount)
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
        protected double SetPendingOrderStopLossPips(int orderCount, int numberOfOrders)
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
        protected int SetVolume(int orderCount)
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

        public void CancelAllPendingOrders()
        {
            //Close any outstanding pending orders
            foreach (PendingOrder po in Bot.PendingOrders)
            {
                try
                {
                    if (BotState.IsThisBotId(po.Label))
                    {
                        Bot.CancelPendingOrderAsync(po, OnCancelPendingOrderOperationComplete);
                    }
                }
                catch (Exception e)
                {
                    Bot.Print("Failed to Cancel Pending Order :" + e.Message);
                }
            }
        }

        protected void OnCancelPendingOrderOperationComplete(TradeResult tr)
        {
            OnPendingOrderOperationComplete(tr, "FAILED to CANCEL pending Order,");
        }

        protected void OnPendingOrderOperationComplete(TradeResult tr, string errorMsg)
        {
            if (!tr.IsSuccessful)
            {
                if (tr.PendingOrder != null)
                {
                    Reporter.ReportTradeResultError(errorMsg + "," + tr.PendingOrder.Label + "," + tr.PendingOrder.TradeType + "," + System.DateTime.Now + "," + tr.Error);
                }
                else
                {
                    Reporter.ReportTradeResultError(tr.Error.ToString());
                }
            }
        }

        protected void OnPositionOperationComplete(TradeResult tr, string errorMsg)
        {
            if (!tr.IsSuccessful)
            {
                if (tr.Position != null)
                {
                    Reporter.ReportTradeResultError(errorMsg + "," + tr.Position.Label + "," + tr.Position.TradeType + "," + System.DateTime.Now + "," + tr.Error);
                }
                else
                {
                    Reporter.ReportTradeResultError(tr.Error.ToString());
                }
            }
        }





    }
}
