using System;

namespace Niffler.Data
{
    class MarketInfo
    {

        public static TimeZoneInfo GetTimeZone(string symbolCode)
        {
            switch (symbolCode)
            {
                case "UK100":
                    return TimeZoneInfo.FindSystemTimeZoneById("GMT Standard Time");

                case "GER30":
                    return TimeZoneInfo.FindSystemTimeZoneById("W. Europe Standard Time");

                case "HK50":
                    return TimeZoneInfo.FindSystemTimeZoneById("China Standard Time");
            }
            return null;
        }


        public static TimeSpan GetMarketOpenTime(string symbolCode)
        {
            switch (symbolCode)
            {
                case "UK100":
                    return new TimeSpan(8, 0, 0);

                case "GER30":
                    return new TimeSpan(9, 0, 0);

                case "HK50":
                    return new TimeSpan(9, 30, 0);
            }
            return TimeSpan.Zero;
        }
    }
}
