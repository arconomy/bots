﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using cAlgo;
using cAlgo.API;
using cAlgo.API.Internals;

namespace Niffler.Common.Market
{
    class Spike
    {
        public double PeakPips { get; set; }
        public double PeakPrice { get; set; }
        public bool PeakCaptured { get; set; }
        private bool IsCaptured { get; set; }

        public void setPeak(double peakPrice, double peakPips)
        {
            if (!IsCaptured)
                IsCaptured = true;

            PeakPrice = peakPrice;
            PeakPips = peakPips;
        }

        public bool isCaptured()
        {
            return IsCaptured;
        }
    }

}
