using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Niffler.Core.Trades
{
    public class TradeUtils
    {
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

        //Increase the volume based on Orders places and volume levels and multiplier until max volume reached
        protected int SetVolume(int orderCount)
        {
            if (!UseVariableVolume)
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

        //Returns the entry distance from the first entry point to an order based on MultiplyOrderSpacingEveryNthOrder, OrderSpacingMultipler and OrderSpacingPips until OrderSpacingMax reached
        protected int CalcOrderSpacingDistance(int orderCount)
        {
            if (!UseVariableOrderSpacing)
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

    }
}
