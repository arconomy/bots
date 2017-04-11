using System;
using System.Linq;
using cAlgo.API;
using cAlgo.API.Indicators;
using cAlgo.API.Internals;
using cAlgo.Indicators;

namespace cAlgo
{
    [Robot(TimeZone = TimeZones.UTC, AccessRights = AccessRights.None)]
    public class FadeTheFillTrade : Robot
    {

        [Parameter("Contract Size", DefaultValue = 1.0)]
        public int ContractSize { get; set; }



        protected override void OnStart()
        {
            // Put your initialization logic here




        }




        protected override void OnBar()
        {
            // Put your core logic here


            TimeSpan FTSEClose = new TimeSpan(16, 30, 0);


            if (MarketSeries.OpenTime.LastValue.DayOfWeek == DayOfWeek.Tuesday || MarketSeries.OpenTime.LastValue.DayOfWeek == DayOfWeek.Thursday || MarketSeries.OpenTime.LastValue.DayOfWeek == DayOfWeek.Wednesday)
            {

                // Check that the last Bar was the Open.
                if (MarketSeries.OpenTime.Last(1).ToUniversalTime().TimeOfDay == new TimeSpan(7, 55, 0))
                {

                    Print("We are currently at the Open.");
                    double BarLength = Math.Abs((MarketSeries.High.Last(1) - MarketSeries.Low.Last(1)));



                    double PreviousClose = 0.0;

                    int PeriodsBack = 0;
                    while (PreviousClose == 0.0)
                    {

                        if (MarketSeries.OpenTime.Last(PeriodsBack).TimeOfDay == FTSEClose)
                        {
                            PreviousClose = MarketSeries.Close.Last(PeriodsBack);
                        }

                        PeriodsBack += 1;
                    }


                    // See if we have Fade to the Fill Positions... 
                    string GapType = "";

                    // GappedUp / Gapped Down
                    if (Symbol.Ask > PreviousClose)
                    {
                        GapType = "GappedUp";
                        double GapDifference = Math.Abs(Symbol.Ask - PreviousClose);
                        // look for Sells...
                        double TakeProfit = (Symbol.Ask - PreviousClose);


                        if (GapDifference > 30)
                        {
                            ExecuteMarketOrder(TradeType.Sell, Symbol, ContractSize, "Flowy", 40, 20);
                        }
                        else
                        {
                            PlaceStopOrder(TradeType.Sell, Symbol, ContractSize, (PreviousClose + 10), "Fade", TakeProfit, TakeProfit, MarketSeries.OpenTime.LastValue.AddHours(2));

                        }



                    }


                    if (Symbol.Bid < PreviousClose)
                    {
                        GapType = "GappedDown";

                        double TakeProfit = (PreviousClose - Symbol.Bid);
                        double GapDifference = Math.Abs(Symbol.Bid - PreviousClose);

                        if (GapDifference > 30)
                        {
                            ExecuteMarketOrder(TradeType.Buy, Symbol, ContractSize, "Flowy", 40, 20);
                        }
                        else
                        {
                            PlaceStopOrder(TradeType.Buy, Symbol, ContractSize, (PreviousClose - 10), "Fade", TakeProfit, TakeProfit, MarketSeries.OpenTime.LastValue.AddHours(2));

                        }


                    }





                }


            }
            else
            {
                Print("Not thie right day for the trade");
            }


        }

        protected override void OnTick()
        {
            // Put your core logic here



            foreach (Position position in Positions)
            {


                if (position.GrossProfit > 10 & position.EntryTime < MarketSeries.OpenTime.LastValue.AddHours(-4))
                    ClosePosition(position);





            }




        }

        protected override void OnStop()
        {
            // Put your deinitialization logic here




        }
    }
}
