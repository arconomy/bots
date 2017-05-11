using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Niffler.Common.Trade
{
    class OrderManager
    {

        //Calculate a new orderCount number for when tick jumps
        protected int calculateNewOrderCount(int orderCount, double currentTickPrice)
        {
            double tickJumpIntoRange = Math.Abs(_openPrice - currentTickPrice) - OrderEntryOffset;
            double pendingOrderRange = calcOrderSpacingDistance(NumberOfOrders);
            double pendingOrdersPercentageJumped = tickJumpIntoRange / pendingOrderRange;
            double newOrderCount = NumberOfOrders * pendingOrdersPercentageJumped;

            if (newOrderCount > orderCount)
                return (int)newOrderCount;
            else
                return (int)orderCount;
        }

        //Returns the distance from the first entry point to an order based on OrderSpacingMultipler and OrderMultiplierLevels until OrderSpacingMax reached
        protected int calcOrderSpacingDistance(int orderCount)
        {
            double orderSpacingLevel = 0;
            double orderSpacing = 0;
            double orderSpacingResult = 0;

            for (int i = 1; i <= orderCount; i++)
            {
                orderSpacingLevel = i / OrderSpacingLevels;
                orderSpacing = Math.Pow(OrderSpacingMultipler, orderSpacingLevel) * OrderSpacing;

                if (orderSpacing > OrderSpacingMax)
                {
                    orderSpacing = OrderSpacingMax;
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
                return FinalOrderStopLoss * (1 / Symbol.TickSize);
            }
            else
            {
                return 0;
            }
        }

        //Increase the volume based on Orders places and volume levels and multiplier until max volume reached
        protected int setVolume(int orderCount)
        {

            double orderVolumeLevel = orderCount / OrderVolumeLevels;
            double volume = Math.Pow(VolumeMultipler, orderVolumeLevel) * Volume;

            if (volume > VolumeMax)
            {
                volume = VolumeMax;
            }

            return (int)volume;
        }

        protected void CloseAllPendingOrders()
        {
            //Close any outstanding pending orders
            foreach (PendingOrder po in PendingOrders)
            {
                try
                {
                    if (isThisBotId(po.Label))
                    {
                        CancelPendingOrderAsync(po, onTradeOperationComplete);
                    }
                }
                catch (Exception e)
                {
                    Print("Failed to Cancel Pending Order :" + e.Message);
                }
            }
            BotState.IsPendingOrdersClosed = true;
        }


    }
}
