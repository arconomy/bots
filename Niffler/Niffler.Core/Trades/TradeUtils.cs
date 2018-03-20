using Niffler.Core.Config;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Niffler.Core.Trades
{
    public class TradeUtils
    {
        private BrokerConfiguration BrokerConfig;
        public TradeVolumeCalculator TradeVolumeCalculator { get; set; }
        public OrderSpacingCalculator OrderSpacingCalculator { get; set; }

        public TradeUtils(BrokerConfiguration brokerConfig)
        {
            this.BrokerConfig = brokerConfig;
        }

        public double CalcPipsForBroker(double pips)
        {
            return pips * BrokerConfig.PipSize;
        }

        public double CalculateVolume(int orderCount)
        {
            return OrderSpacingCalculator.Calculate(orderCount);
        }






        //Calculate a new orderCount number for when tick jumps
        public int CalcNewOrderCount(int orderCount, double currentTickPrice)
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


       

    }
}
