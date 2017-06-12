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
        public static string GetTimeStamp(Robot bot, bool unformatted = false)
        {

            DateTime datetime = System.DateTime.Now;
            if (bot.IsBacktesting)
                datetime = bot.Server.Time;

            if (unformatted)
                return datetime.Year.ToString() + datetime.Month + datetime.Day + datetime.Minute + datetime.Second;
            return datetime.Year + "-" + datetime.Month + "-" + datetime.Day;
        }
    }
}
