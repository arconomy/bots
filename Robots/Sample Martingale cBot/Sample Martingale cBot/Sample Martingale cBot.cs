// -------------------------------------------------------------------------------------------------
//
//    This code is a cAlgo API sample.
//
//    This cBot is intended to be used as a sample and does not guarantee any particular outcome or
//    profit of any kind. Use it at your own risk
//
//    The "Sample Martingale cBot" creates a random Sell or Buy order. If the Stop loss is hit, a new 
//    order of the same type (Buy / Sell) is created with double the Initial Volume amount. The cBot will 
//    continue to double the volume amount for  all orders created until one of them hits the take Profit. 
//    After a Take Profit is hit, a new random Buy or Sell order is created with the Initial Volume amount.
//
// -------------------------------------------------------------------------------------------------

using System;
using System.Linq;
using cAlgo.API;
using cAlgo.API.Indicators;
using cAlgo.API.Internals;
using cAlgo.Indicators;

namespace cAlgo
{
    [Robot(TimeZone = TimeZones.UTC, AccessRights = AccessRights.None)]
    public class SampleMartingalecBot : Robot
    {
        [Parameter("Initial Quantity (Lots)", DefaultValue = 1, MinValue = 0.01, Step = 0.01)]
        public double InitialQuantity { get; set; }

        [Parameter("Stop Loss", DefaultValue = 40)]
        public int StopLoss { get; set; }

        [Parameter("Take Profit", DefaultValue = 40)]
        public int TakeProfit { get; set; }


        [Parameter("EMA", DefaultValue = 14)]
        public int EmaValue { get; set; }


        [Parameter()]
        public DataSeries SourceSeries { get; set; }


        protected override void OnStart()
        {
            Positions.Closed += OnPositionsClosed;

            ExecuteOrder(InitialQuantity);
        }

        private void ExecuteOrder(double quantity)
        {

            ExponentialMovingAverage EMA50 = Indicators.ExponentialMovingAverage(SourceSeries, EmaValue);

            TradeType tradeType = TradeType.Sell;
            if (EMA50.Result.Last(0) > EMA50.Result.Last(5))
                tradeType = TradeType.Buy;



            var volumeInUnits = Symbol.QuantityToVolume(quantity);
            var result = ExecuteMarketOrder(tradeType, Symbol, volumeInUnits, "Martingale", StopLoss, TakeProfit);

        }

        private void OnPositionsClosed(PositionClosedEventArgs args)
        {
            Print("Closed");
            var position = args.Position;

            //  if (position.Label != "Martingale" || position.SymbolCode != Symbol.Code)
            //    return;

            if (position.GrossProfit >= 0)
            {


                ExecuteOrder(position.Quantity);


            }

        }


    }
}
