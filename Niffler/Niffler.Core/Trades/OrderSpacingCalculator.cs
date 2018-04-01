using System;
using Niffler.Core.Config;
using Niffler.Common;

namespace Niffler.Core.Trades
{
    public class OrderSpacingCalculator
    {
        private bool Intialised = false;
        public bool EnableDynamicOrderSpacing;
        private double OrderSpacingBasePips;
        private double OrderSpacingMaxPips;
        private double OrderSpacingIncrementPips;
        private double IncrementSpacingAfterOrders;
        private int NumberOfOrders;

        public OrderSpacingCalculator(RuleConfiguration ruleConfig)
        {
            this.Intialised = Initilise(ruleConfig);
        }

        private bool Initilise(RuleConfiguration ruleConfig)
        {
            OrderSpacingConfiguration OrderSpacingConfiguration = null;
            if (Utils.GetRuleConfigOrderSpacing(ruleConfig, ref OrderSpacingConfiguration)) return false;

            this.EnableDynamicOrderSpacing = OrderSpacingConfiguration.EnableDynamicOrderSpacing;
            this.OrderSpacingBasePips = OrderSpacingConfiguration.OrderSpacingBasePips;
            this.OrderSpacingMaxPips = OrderSpacingConfiguration.OrderSpacingMaxPips;
            this.OrderSpacingIncrementPips = OrderSpacingConfiguration.OrderSpacingIncrementPips;
            this.IncrementSpacingAfterOrders = OrderSpacingConfiguration.IncrementSpacingAfterOrders;
            if (Utils.GetRuleConfigIntegerParam(RuleConfiguration.NUMBEROFORDERS, ruleConfig, ref NumberOfOrders)) return false;
            return true;
        }

        public int GetOrderNumberFromPips(double pipsIntoRange)
        {
            if (!EnableDynamicOrderSpacing)
            {
                return (int) Math.Ceiling(pipsIntoRange/OrderSpacingBasePips);
            }

            double orderSpacingDistancePips = 0;
            for (int orderNumber = 1; orderNumber <= NumberOfOrders; orderNumber++)
            {
                orderSpacingDistancePips += GetOrderSpacingPips(orderNumber);
                if (orderSpacingDistancePips > pipsIntoRange)
                    return orderNumber;
            }
            //return the last order number if the pips are outside the total distance for range of orders
            return NumberOfOrders;
        }
        
        public double GetOrderSpacingDistancePips(int currentOrderNumber)
        {
            if (!EnableDynamicOrderSpacing)
            {
                return (int)currentOrderNumber * OrderSpacingBasePips;
            }

            double nextOrderSpacingLimitPips = 0;

            //The spacing distance required is the orderspacing distance limit for the previous order
            for (int orderNumber = 1; orderNumber < currentOrderNumber; orderNumber++)
            {
                nextOrderSpacingLimitPips += GetOrderSpacingPips(orderNumber);
            }
            return nextOrderSpacingLimitPips;
        }


        private double GetOrderSpacingPips(int orderNumber)
        {

            double orderSpacingLevel = Math.Ceiling(orderNumber / IncrementSpacingAfterOrders);
            double orderSpacingPips = OrderSpacingIncrementPips * (orderSpacingLevel - 1);

            if (orderSpacingPips < 1)
            {
                orderSpacingPips = OrderSpacingBasePips;
            }

            if (orderSpacingPips > OrderSpacingMaxPips)
            {
                orderSpacingPips = OrderSpacingMaxPips;
            }

            return orderSpacingPips;
        }
    }
}
