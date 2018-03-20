using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Niffler.Core.Trades
{
    public class OrderSpacingCalculator
    {
        private bool EnableOrderSpacing;
        private double OrderSpacingBasePips;
        private double OrderSpacingMaxPips;
        private double OrderSpacingIncrementPips;
        private double IncrementSpacingAferOrders;

        public OrderSpacingCalculator(bool enableOrderSpacing, double orderSpacingBasePips, double orderSpacingMaxPips, double orderSpacingIncrementPips, double incrementSpacingAferOrders)
        {
            this.EnableOrderSpacing = enableOrderSpacing;
            this.OrderSpacingBasePips = orderSpacingBasePips;
            this.OrderSpacingMaxPips = orderSpacingMaxPips;
            this.OrderSpacingIncrementPips = orderSpacingIncrementPips;
            this.IncrementSpacingAferOrders = incrementSpacingAferOrders;
        }

        //Returns the entry distance from the first entry point to an order based on MultiplyOrderSpacingEveryNthOrder, OrderSpacingMultipler and OrderSpacingPips until OrderSpacingMax reached
        public double Calculate(int orderCount)
        {
            if (!EnableOrderSpacing)
            {
                return OrderSpacingBasePips;
            }

            double orderSpacingLevel = 0;
            double orderSpacing = 0;
            double orderSpacingResult = 0;

            for (int i = 1; i <= orderCount; i++)
            {
                orderSpacingLevel = i / IncrementSpacingAferOrders;
                orderSpacing = Math.Pow(OrderSpacingIncrementPips, orderSpacingLevel) * OrderSpacingIncrementPips;

                if (orderSpacing > OrderSpacingMaxPips)
                {
                    orderSpacing = OrderSpacingMaxPips;
                }

                orderSpacingResult += orderSpacing;
            }
            return orderSpacingResult;
        }
    }
}
