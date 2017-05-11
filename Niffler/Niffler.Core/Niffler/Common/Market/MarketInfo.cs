using System;
using System.Linq;
using cAlgo.API;
using cAlgo.API.Indicators;
using cAlgo.API.Internals;
using cAlgo.Indicators;

namespace Niffler.Common.Market
{
    class MarketInfo
    {
        private String market;
        public TimeZoneInfo tz;
        public TimeSpan open;
        public TimeSpan close;
        public TimeSpan closeAll;


        protected bool IsSwordFishTime()
        {
            return _swordFishTimeInfo.IsPlacePendingOrdersTime(IsBacktesting, Server.Time);
        }

        //Is the current time within the period Swordfish Pending Orders can be placed
        public bool IsPlacePendingOrdersTime(bool isBackTesting, DateTime serverTime)
        {
            if (isBackTesting)
            {
                return IsOpenAt(serverTime);
            }
            else
            {
                return IsOpenAt(DateTime.UtcNow);
            }
        }

        //Time during which Swordfish positions risk should be managed
        public bool IsReduceRiskTime(bool isBackTesting, DateTime serverTime, int reduceRiskTimeFromOpen)
        {
            if (isBackTesting)
            {
                return IsReduceRiskAt(serverTime, reduceRiskTimeFromOpen);
            }
            else
            {
                return IsReduceRiskAt(DateTime.UtcNow, reduceRiskTimeFromOpen);
            }
        }

        //Is the current time within the period Swordfish positions can remain open.
        public bool IsCloseAllPositionsTime(bool isBackTesting, DateTime serverTime)
        {

            if (isBackTesting)
            {
                return IsCloseAllAt(serverTime);
            }
            else
            {
                return IsCloseAllAt(DateTime.UtcNow);
            }
        }

        //Is the current time within the period Swordfish Pending Orders can be placed.
        public bool IsOpenAt(DateTime dateTimeUtc)
        {
            DateTime tzTime = TimeZoneInfo.ConvertTimeFromUtc(dateTimeUtc, tz);
            return (tzTime.TimeOfDay >= open & tzTime.TimeOfDay <= close);
        }

        //Is the current time after the time period when risk should be reduced.
        public bool IsReduceRiskAt(DateTime dateTimeUtc, int reduceRiskTimeFromOpen)
        {
            DateTime tzTime = TimeZoneInfo.ConvertTimeFromUtc(dateTimeUtc, tz);
            return (tzTime.TimeOfDay >= open.Add(TimeSpan.FromMinutes(reduceRiskTimeFromOpen)));
        }

        //Is the current time within the period Swordfish positions can remain open.
        public bool IsCloseAllAt(DateTime dateTimeUtc)
        {
            DateTime tzTime = TimeZoneInfo.ConvertTimeFromUtc(dateTimeUtc, tz);
            return tzTime.TimeOfDay >= closeAll;
        }

        protected void setTimeZone()
        {

            switch (Symbol.Code)
            {
                case "UK100":
                    // Instantiate a MarketTimeInfo object.
                    _swordFishTimeInfo.market = "FTSE";
                    _swordFishTimeInfo.tz = TimeZoneInfo.FindSystemTimeZoneById("GMT Standard Time");
                    // Market for swordfish trades opens at 8:00am.
                    _swordFishTimeInfo.open = new TimeSpan(8, 0, 0);
                    // Market for swordfish trades closes at 8:05am.
                    _swordFishTimeInfo.close = new TimeSpan(8, 5, 0);
                    // Close all open Swordfish position at 11:29am before US opens.
                    _swordFishTimeInfo.closeAll = new TimeSpan(11, 29, 0);

                    break;
                case "GER30":
                    _swordFishTimeInfo.market = "DAX";
                    _swordFishTimeInfo.tz = TimeZoneInfo.FindSystemTimeZoneById("W. Europe Standard Time");
                    // Market for swordfish opens at 9:00.
                    _swordFishTimeInfo.open = new TimeSpan(9, 0, 0);
                    // Market for swordfish closes at 9:05.
                    _swordFishTimeInfo.close = new TimeSpan(9, 3, 0);
                    // Close all open Swordfish position at 11:29am before US opens.
                    _swordFishTimeInfo.closeAll = new TimeSpan(11, 29, 0);
                    break;
                case "HK50":
                    _swordFishTimeInfo.market = "HSI";
                    _swordFishTimeInfo.tz = TimeZoneInfo.FindSystemTimeZoneById("China Standard Time");
                    // Market for swordfish opens at 9:00.
                    _swordFishTimeInfo.open = new TimeSpan(9, 30, 0);
                    // Market for swordfish closes at 9:05.
                    _swordFishTimeInfo.close = new TimeSpan(9, 35, 0);
                    // Close all open Swordfish positions
                    _swordFishTimeInfo.closeAll = new TimeSpan(11, 30, 0);
                    break;
            }
        }


    }
}
