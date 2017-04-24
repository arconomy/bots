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


        [Parameter("Trader Name")]
        public override string TraderName { get; set; }

        [Parameter("Trader Email")]
        public override string TraderEmail { get; set; }

        // Niffler Client Bot

        // This Bot is used to analyze the market data and your positions. It needs to
        // be on and running while you have positions open/or while you're trading for the best impact.

        // i.e. - It will scan Markets for Cash hour Open/Close/High/Low and update your Daily Levels
        //      - It will manage any Advanced Trailing Stop losses
        //      - It will monitor the markets for Trade Setups that you can specify

        // Enjoy!

    }
}
