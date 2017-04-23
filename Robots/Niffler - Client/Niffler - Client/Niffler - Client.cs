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
    public class NifflerClient : Niffler.Bots.Client.Bot
    {


        [Parameter("Test Parameter")]
        public override double TestParameter { get; set; }

        // Niffler Client Bot

        // This Bot is used to analyze the market data and your positions. It needs to
        // be on and running while you have positions open/or while you're trading. 

        // Enjoy!

    }
}
