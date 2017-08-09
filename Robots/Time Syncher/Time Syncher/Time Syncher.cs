using System;
using System.Linq;
using cAlgo.API;
using cAlgo.API.Indicators;
using cAlgo.API.Internals;
using cAlgo.Indicators;

namespace cAlgo
{
    [Robot(TimeZone = TimeZones.UTC, AccessRights = AccessRights.None)]
    public class TimeSyncher : Robot
    {
        protected override void OnStart()
        {

        }

        protected override void OnTick()
        {
            Print("System Clock : " + System.DateTime.UtcNow);
            Print("Server Clock : " + Server.Time);
            Print("---------------------------------");
        }

        protected override void OnStop()
        {
            // Put your deinitialization logic here
        }
    }
}
