using System;
using cAlgo.API;
using Niffler.Messaging.Protobuf;

namespace Niffler.Common
{
    class Utils
    {
        private static DateTime GetDateTimeToUse(Tick tick)
        {
            DateTime datetime;
            if (tick.Isbacktesting)
            {
                if (!DateTime.TryParse(tick.Timestamp, out datetime))
                    datetime = new DateTime(TimeSpan.Zero.Milliseconds); //Set DateTime to default 00:00:00
            }
            else
                datetime = System.DateTime.Now;

            return datetime;
        }

        public static string GetTimeStamp(bool formatWithSeperators = false)
        {
            DateTime datetime = System.DateTime.Now;

            if (formatWithSeperators)
                return FormatDateTimeWithSeparators(datetime);

            return FormatDateTime(datetime);
        }

        public static string GetTimeStamp(Tick tick, bool formatWithSeperators = false)
        {
            DateTime datetime = GetDateTimeToUse(tick);

            if (formatWithSeperators)
                return FormatDateTimeWithSeparators(datetime);

            return FormatDateTime(datetime);
        }

        public static string FormatDateTime(DateTime datetime)
        {
            return datetime.Year.ToString() + datetime.Month + datetime.Day + datetime.Hour + datetime.Minute + datetime.Second;
        }

        public static string FormatDateTimeWithSeparators(DateTime datetime)
        {
            return datetime.Day + "-" + datetime.Month + "-" + datetime.Year.ToString() + " " + datetime.Hour + ":" + datetime.Minute + ":" + datetime.Second;
        }

        public static string GetUniqueID()
        {
            DateTime datetime = System.DateTime.Now;
            return datetime.Year.ToString() + datetime.Month + datetime.Day + datetime.Hour + datetime.Minute + datetime.Second;
        }
    }
}
