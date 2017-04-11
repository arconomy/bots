using System;
using System.Linq;
using cAlgo.API;
using cAlgo.API.Indicators;
using cAlgo.API.Internals;
using cAlgo.Indicators;

namespace cAlgo
{
    [Robot(TimeZone = TimeZones.UTC, AccessRights = AccessRights.None)]
    public class MajorsBot : Robot
    {
        [Parameter("Modify Period", DefaultValue = 2)]
        public int ModifyPeriod { get; set; }

        [Parameter("Stop Loss", DefaultValue = 30)]
        public int StopLoss { get; set; }

        [Parameter("Take Profit", DefaultValue = 30)]
        public int TakeProfit { get; set; }

        [Parameter("Entry Offset", DefaultValue = 30)]
        public int EntryOffset { get; set; }

        [Parameter("Volume", DefaultValue = 10)]
        public int Volume { get; set; }


        protected override void OnStart()
        {

            Print("Started Timer");
            Timer.Start(ModifyPeriod);
        }


        protected override void OnTimer()
        {


            Print("Tick");

            // Create new Pending Orders if they dont already exist.... 
            bool HasBuyPosition = false;
            bool HasSellPosition = false;
            foreach (Position position in Positions)
            {

                if (position.SymbolCode != Symbol.Code)
                    continue;

                if (position.Label == "BUY")
                {
                    HasBuyPosition = true;
                }
                else
                {
                    HasSellPosition = true;
                }
            }


            //If we dont have a Buy Position...
            if (!HasBuyPosition)
            {

                // IF we have a Pending BUY then MOdify...
                bool HasBuyOrder = false;
                foreach (PendingOrder PO in PendingOrders)
                {
                    if (PO.SymbolCode != Symbol.Code)
                        continue;

                    if (PO.TradeType == TradeType.Buy)
                    {
                        HasBuyOrder = true;
                        ModifyPendingOrder(PO, (Symbol.Ask + EntryOffset), StopLoss, TakeProfit, null);
                    }
                }
                // If we dont have a Pending Buy then create a new one!
                if (!HasBuyOrder)
                {
                    var Result = PlaceStopOrder(TradeType.Buy, Symbol, Volume, (Symbol.Ask + EntryOffset), "BUY", StopLoss, TakeProfit);

                }

            }

            //Make some monay!!

            if (!HasSellPosition)
            {
                bool HasSellOrder = false;
                foreach (PendingOrder PO in PendingOrders)
                {

                    if (PO.SymbolCode != Symbol.Code)
                        continue;

                    if (PO.TradeType == TradeType.Sell)
                    {
                        HasSellOrder = true;
                        ModifyPendingOrder(PO, (Symbol.Bid - EntryOffset), StopLoss, TakeProfit, null);
                    }
                }


                //Modify Order
                if (!HasSellOrder)
                    PlaceStopOrder(TradeType.Sell, Symbol, Volume, (Symbol.Bid - EntryOffset), "SELL", StopLoss, TakeProfit);


            }



        }

        protected override void OnTick()
        {
            // Put your core logic here




        }

        protected override void OnStop()
        {
            // Put your deinitialization logic here
        }
    }
}
