using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Niffler.Common
{
    class Utils
    {
        public static string GetTimeStamp(bool unformatted = false)
        {
            if (unformatted)
                return System.DateTime.Now.Year.ToString() + System.DateTime.Now.Month + System.DateTime.Now.Day + System.DateTime.Now.Minute + System.DateTime.Now.Second;
            return System.DateTime.Now.Year + "-" + System.DateTime.Now.Month + "-" + System.DateTime.Now.Day;
        }
    }
}
