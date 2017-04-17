using System;
using System.Linq;
using cAlgo.API;
using cAlgo.API.Indicators;
using cAlgo.API.Internals;
using cAlgo.Indicators;
using BottyTest;

namespace cAlgo
{
    [Robot(TimeZone = TimeZones.UTC, AccessRights = AccessRights.FullAccess)]
    public class Client : Robot
    {
        [Parameter(DefaultValue = 0.0)]
        public double Parameter { get; set; }

        protected override void OnStart()
        {

            // Sync Account and Set Access
            Botty.CheckIn(Account);

            // Run hourly tasks
            Timer.Start(new TimeSpan(0, 0, 10));




        }



        protected override void OnError(Error error)
        {


        }

        protected override void OnPositionClosed(Position position)
        {
            Botty.TrackPosition(Account, position, "Closed");

        }

        protected override void OnPositionOpened(Position openedPosition)
        {
            Botty.TrackPosition(Account, openedPosition, "Opened");
        }

        protected override void OnPendingOrderCreated(PendingOrder newOrder)
        {


        }


        protected override void OnBar()
        {


        }

        protected override void OnTimer()
        {


            // Monitor and Calculate Daily Levels
            // if there are no daily levels for today, and if the market has closed for today then collect todays!

            BottyTest.Indicators.KeyLevelScraper.Update(Account, Symbol, MarketData.GetSeries(TimeFrame.Minute30));

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
