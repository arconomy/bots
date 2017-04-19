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

            Positions.Closed += OnPositionsClosed;
            Positions.Opened += OnPositionsOpened;

            Niffler.Bots.Client.Entry.UpdateAccountAndPositions(Account, Positions);

            Timer.Start(new TimeSpan(0, 0, 10));

        }

        void OnPositionsOpened(PositionOpenedEventArgs obj)
        {
            Niffler.Bots.Client.Entry.UpdatePosition(Account, obj.Position, "Opened");
        }

        void OnPositionsClosed(PositionClosedEventArgs obj)
        {
            Niffler.Bots.Client.Entry.UpdatePosition(Account, obj.Position, "Closed");
        }

        protected override void OnError(Error error)
        {
            // Pending...
        }

        protected override void OnPendingOrderCreated(PendingOrder newOrder)
        {
            // nothing required...
        }

        protected override void OnBar()
        {
            // Nothing Required?
        }

        protected override void OnTimer()
        {
            Niffler.Bots.Client.Entry.UpdateAccountAndPositions(Account, Positions);
            Niffler.Bots.Client.Entry.UpdateKeyLevels(Account, Symbol, MarketData.GetSeries(TimeFrame.Minute30));
        }

        protected override void OnTick()
        {
            // nothing required...
        }

        protected override void OnStop()
        {
            // nothing required...
        }
    }
}
