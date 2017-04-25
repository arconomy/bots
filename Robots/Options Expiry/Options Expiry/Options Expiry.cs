using System;
using System.Linq;
using cAlgo.API;
using cAlgo.API.Indicators;
using cAlgo.API.Internals;
using cAlgo.Indicators;

namespace cAlgo
{
    [Robot(TimeZone = TimeZones.GMTStandardTime, AccessRights = AccessRights.None)]
    public class OptionsExpiry : Robot
    {


        // OPTIONS EXPIRY TRADE

        // Written by: Andrew
        // Backtested by: N/A

        // INSTRUCTIONS
        // Turn on before the Trade Takes Place (anytime that day or that week before).
        // Turn off anytime before the next friday.
        // It will ONLY trade on a Friday at 10:10am (GMT). 
        // So if the Options is not on a Friday you need to modify the bot.



        [Parameter("Contract Size", DefaultValue = 1)]
        public int ContractSize { get; set; }

        [Parameter("Pending Order Validity (mins)", DefaultValue = 7)]
        public int PendingOrderMins { get; set; }

        [Parameter("Minimum Movement for Validation", DefaultValue = 8)]
        public int MinMovementForValidity { get; set; }

        [Parameter("Gaps between Limit Orders", DefaultValue = 1)]
        public int GapsBetweenOrders { get; set; }

        [Parameter("Number of Orders", DefaultValue = 10)]
        public int NumberOfOrders { get; set; }

        [Parameter("Take Proft (Pts.)", DefaultValue = 6)]
        public int TakeProfit { get; set; }



        protected override void OnStart()
        {
            // Put your initialization logic here


        }


        protected override void OnPositionOpened(Position openedPosition)
        {

            // when the first position is opened (either up or down) close off the other pending Orders on the other side.
            foreach (PendingOrder P in PendingOrders)
            {
                if (P.TradeType != openedPosition.TradeType)
                {
                    CancelPendingOrder(P);
                    Print("Cancelling Orders that are for the other direction.");
                }
            }



        }

        protected override void OnBar()
        {


            if (MarketSeries.OpenTime.LastValue.TimeOfDay == new TimeSpan(10, 10, 0) && MarketSeries.OpenTime.LastValue.DayOfWeek == DayOfWeek.Friday)
            {


                double OpeningPrice = MarketSeries.Open.LastValue;

                Print("On Options Expiry Opening Bar which is: " + OpeningPrice.ToString());

                TakeProfit = (int)(TakeProfit * (1 / Symbol.TickSize));

                // If the Options Move down we're looking for Buys...
                double SellStart = OpeningPrice - MinMovementForValidity;
                for (int i = 0; i <= (NumberOfOrders - 1); i += GapsBetweenOrders)
                {

                    Print("Starting buy loop");
                    double Entry = SellStart - i;
                    //  double StopLoss; null as we dont know where it will be....  Semi Manual Bot... 
                    //  var result = PlaceLimitOrder(TradeType.Buy, Symbol, ContractSize, Entry, "BUY", null, TakeProfit, MarketSeries.OpenTime.LastValue.AddMinutes(PendingOrderMins));

                    PlaceLimitOrderAsync(TradeType.Buy, Symbol, ContractSize, Entry, "BUY", null, TakeProfit, MarketSeries.OpenTime.LastValue.AddMinutes(PendingOrderMins), "BarBreak");

                    //if (!result.IsSuccessful)
                    //    Print(result.Error);

                    Print("BUY at " + Entry.ToString() + ", TP: " + TakeProfit.ToString());

                }


                //If the Options move up we're looking for sells....
                double BuyStart = OpeningPrice + MinMovementForValidity;
                for (int i = 0; i <= (NumberOfOrders - 1); i += GapsBetweenOrders)
                {

                    Print("Starting sell loop");
                    double Entry = BuyStart + i;
                    //  double StopLoss; null as we dont know where it will be....  Semi Manual Bot... 
                    //  var result = PlaceLimitOrder(TradeType.Sell, Symbol, ContractSize, Entry, "SELL", null, TakeProfit, MarketSeries.OpenTime.LastValue.AddMinutes(PendingOrderMins));

                    PlaceLimitOrderAsync(TradeType.Sell, Symbol, ContractSize, Entry, "SELL", null, TakeProfit, MarketSeries.OpenTime.LastValue.AddMinutes(PendingOrderMins), "BarBreak");

                    //  if (!result.IsSuccessful)
                    //    Print(result.Error);

                    Print("SELL at " + Entry.ToString() + ", TP: " + TakeProfit.ToString());

                }


            }

        }


        protected override void OnTick()
        {

            // Manage any open Positions.



        }

        protected override void OnStop()
        {
            // Put your deinitialization logic here

        }



        // -------------------------------------------------------------------------------------------


        protected bool LevelAvailable(double Level)
        {

            Level = Math.Floor(Level);

            foreach (Position Position in Positions)
            {

                // if Mod or floored entry price = Level  
                if ((Math.Floor(Position.EntryPrice) % 3) == 0)
                {
                    if (Math.Floor(Position.EntryPrice) == Level)
                        return false;

                }

            }

            return true;



        }








    }
}
