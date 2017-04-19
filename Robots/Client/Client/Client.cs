using System;
using System.Linq;
using cAlgo.API;
using cAlgo.API.Indicators;
using cAlgo.API.Internals;
using cAlgo.Indicators;
using Niffler;

namespace cAlgo
{
    [Robot(TimeZone = TimeZones.UTC, AccessRights = AccessRights.FullAccess)]
    public class Client : Robot
    {

        protected override void OnStart()
        {

            // Sync Account and Set Access
            Niffler.Bots.Client.Entry.UpdateAccount(Account);

            // Run hourly tasks
            Timer.Start(new TimeSpan(0, 0, 10));

        }



        protected override void OnError(Error error)
        {


        }

        protected override void OnPositionClosed(Position closedposition)
        {
            // Check Positions to Manage
            Niffler.Bots.Client.Entry.UpdatePosition(Account, closedposition, "Closed");
        }

        protected override void OnPositionOpened(Position openedPosition)
        {
            // Check Positions to Manage
            Niffler.Bots.Client.Entry.UpdatePosition(Account, openedPosition, "Opened");

        }

        protected override void OnPendingOrderCreated(PendingOrder newOrder)
        {


        }


        protected override void OnBar()
        {


        }

        protected override void OnTimer()
        {

            // Check on Positions
            Niffler.Bots.Client.Entry.UpdateAccountAndPositions(Account, Positions);

            // Monitor and Calculate Daily Levels 
            Niffler.Bots.Client.Entry.UpdateKeyLevels(Account, Symbol, MarketData.GetSeries(TimeFrame.Minute30));



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
