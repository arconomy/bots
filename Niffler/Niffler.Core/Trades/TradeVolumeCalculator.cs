using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Niffler.Core.Trades
{
    public class TradeVolumeCalculator
    {
        private bool EnableVolumeIncrease;
        private bool UseVolumeMultiplier;
        private double VolumeBase;
        private double VolumeMax;
        private double VolumeMultiplier;
        private double VolumeIncrement;
        private double IncreaseVolumeAfterOrders;

        public TradeVolumeCalculator(bool enableVolumeIncrease, bool useVolumeMultiplier, double volumeBase, double volumeMax, double volumeAdjustmentFactor, double increaseVolumeAfterOrders)
        {
            this.EnableVolumeIncrease = enableVolumeIncrease;
            this.UseVolumeMultiplier = useVolumeMultiplier;
            this.VolumeBase = volumeBase;
            this.VolumeMax = volumeMax;
            this.IncreaseVolumeAfterOrders = increaseVolumeAfterOrders;

            if (useVolumeMultiplier)
            {
                this.VolumeMultiplier = volumeAdjustmentFactor;
            }
            else
            {
                this.VolumeIncrement = volumeAdjustmentFactor;
            }
        }

        //Increase the volume based on Orders places and volume levels and multiplier until max volume reached
        public double Calculate(int orderCount)
        {
            if (!EnableVolumeIncrease)
            {
                return VolumeBase;
            }

            if (UseVolumeMultiplier)
            {
                return CalculateMultipliedVolume(orderCount);
            }
            else
            {
                return CalculateIncrementedVolume(orderCount);
            }
        }

        private double CalculateMultipliedVolume(int orderCount)
        {
            double orderVolumeLevel = orderCount / IncreaseVolumeAfterOrders;
            double volume = Math.Pow(VolumeMultiplier, orderVolumeLevel) * VolumeBase;

            if (volume > VolumeMax)
            {
                volume = VolumeMax;
            }

            return volume;
        }

        private double CalculateIncrementedVolume(int orderCount)
        {
            double orderVolumeLevel = orderCount / IncreaseVolumeAfterOrders;
            double volume = Math.Pow(VolumeMultiplier, orderVolumeLevel) * VolumeBase; //NEED TO CALCULATE THE INCREMENTED VOLUME

            if (volume > VolumeMax)
            {
                volume = VolumeMax;
            }

            return volume;
        }

    }

}
