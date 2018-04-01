using Niffler.Common;
using Niffler.Core.Config;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Niffler.Core.Trades
{
    public class TradeVolumeCalculator
    {
        private bool Intialised = false;
        private bool EnableDynamicVolumeIncrease = false;

        public enum CalculationType
        {
            Multiply = 0,
            Increment = 1
        }

        private CalculationType Type;
        private bool UseVolumeMultiplier;
        private double VolumeBase;
        private double VolumeMax;
        private double VolumeIncreaseFactor;
        private double IncreaseVolumeAfterOrders;

        public TradeVolumeCalculator(RuleConfiguration ruleConfig)
        {
            this.Intialised = Initilise(ruleConfig);
        }

        private bool Initilise(RuleConfiguration ruleConfig)
        {
            VolumeConfiguration VolumeConfiguration = null;
            if (Utils.GetRuleConfigVolume(ruleConfig, ref VolumeConfiguration)) return false;

            this.EnableDynamicVolumeIncrease = VolumeConfiguration.EnableDynamicVolumeIncrease;
            this.Type = VolumeConfiguration.Type;
            this.VolumeBase = VolumeConfiguration.VolumeBase;
            this.VolumeMax = VolumeConfiguration.VolumeMax;
            this.IncreaseVolumeAfterOrders = VolumeConfiguration.IncreaseVolumeAfterOrders;
            this.VolumeIncreaseFactor = VolumeConfiguration.VolumeIncreaseFactor;
            return true;
        }

        public double GetNextOrderVolume(int orderNumber)
        {
            if (!EnableDynamicVolumeIncrease)
            {
                return VolumeBase;
            }

            if (UseVolumeMultiplier)
            {
                return CalculateMultipliedVolume(orderNumber);
            }
            else
            {
                return CalculateIncrementedVolume(orderNumber);
            }
        }

        private double CalculateMultipliedVolume(int currentOrderNumber)
        {
            double volumeLevel = Math.Ceiling(currentOrderNumber / IncreaseVolumeAfterOrders);
            double volume = Math.Ceiling(Math.Pow(VolumeIncreaseFactor, volumeLevel-1) * VolumeBase);

            if (volume > VolumeMax)
            {
                return VolumeMax;
            }

            return volume;
        }

        private double CalculateIncrementedVolume(int currentOrderNumber)
        {
            double volumeLevel = Math.Ceiling(currentOrderNumber / IncreaseVolumeAfterOrders);
            double volumeIncrease = VolumeIncreaseFactor * (volumeLevel - 1);
            if (volumeIncrease < 1)
            {
                return VolumeBase;
            }

            double orderVolume = VolumeBase + volumeIncrease;

            if (orderVolume > VolumeMax)
            {
                return VolumeMax;
            }
            
            return orderVolume;
        }
    }

}
