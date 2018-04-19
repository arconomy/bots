using Niffler.Common;
using Niffler.Core.Config;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static Niffler.Messaging.Protobuf.Position.Types;

namespace Niffler.Core.Trades
{
    public class TradeUtils
    {
        private BrokerConfiguration BrokerConfig;
        private TradeVolumeCalculator TradeVolumeCalculator { get; set; }
        private OrderSpacingCalculator OrderSpacingCalculator { get; set; }

        public TradeUtils(BrokerConfiguration brokerConfig, RuleConfiguration ruleConfig)
        {
            this.BrokerConfig = brokerConfig;

            TradeVolumeCalculator = new TradeVolumeCalculator(ruleConfig);
            OrderSpacingCalculator = new OrderSpacingCalculator(ruleConfig);
        }
        
        public double CalcPipsForBroker(double pips)
        {
            return pips * BrokerConfig.PipSize;
        }

        public double AddPipsToPrice(double price, double pips)
        {
            return price + (pips / BrokerConfig.PipSize);
        }

        public double SubtractPipsFromPrice(double price, double pips)
        {
            return price - (pips / BrokerConfig.PipSize);
        }
        

        //Calculate a next orderCount number when tick sequence gaps
        public int CalculateNextOrderNumber(TradeType tradeType, double currentTickPrice, double firstEntryPrice)
        {
            double currentPipsIntoRange = 0;
            switch (tradeType)
            {
                case TradeType.Buy:
                    currentPipsIntoRange = firstEntryPrice - currentTickPrice;
                break;
                case TradeType.Sell:
                    currentPipsIntoRange = currentTickPrice - firstEntryPrice;
                    break;

            }
            currentPipsIntoRange = CalcPipsForBroker(currentPipsIntoRange);
            return OrderSpacingCalculator.GetOrderNumberFromPips(currentPipsIntoRange);
        }

        public double CalculateNextOrderVolume(int orderNumber)
        {
            return TradeVolumeCalculator.GetNextOrderVolume(orderNumber);
        }

        public double CalculateNextEntryPips(int orderNumber)
        {
            return OrderSpacingCalculator.GetOrderSpacingDistancePips(orderNumber);
        }
    }
}
