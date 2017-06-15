using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using cAlgo.API;

namespace Niffler.Common
{
    class Utils
    {

        private static DateTime GetDateTimeToUse(Robot bot)
        {
            DateTime datetime = System.DateTime.Now;
            if (bot.IsBacktesting)
                datetime = bot.Server.Time;

            return datetime;
        }


        public static string GetTimeStamp(Robot bot, bool unformatted = false)
        {

            DateTime datetime = GetDateTimeToUse(bot);

            if (unformatted)
                return datetime.Year.ToString() + datetime.Month + datetime.Day + datetime.Hour + datetime.Minute + datetime.Second;
            return datetime.Day + "-" + datetime.Month + "-" + datetime.Year.ToString() + " " + datetime.Hour + ":" + datetime.Minute + ":" + datetime.Second;
        }

        public static string GetDayOfWeek(Robot bot)
        {
            DateTime datetime = GetDateTimeToUse(bot);
            return datetime.DayOfWeek.ToString();
        }

    }
}
