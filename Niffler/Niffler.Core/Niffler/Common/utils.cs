using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Niffler.Common
{
    class Utils
    {
        public static string getTimeStamp(bool unformatted = false)
        {
            if (unformatted)
                return System.DateTime.Now.Year.ToString() + System.DateTime.Now.Month + System.DateTime.Now.Day + System.DateTime.Now.Minute + System.DateTime.Now.Second;
            return System.DateTime.Now.Year + "-" + System.DateTime.Now.Month + "-" + System.DateTime.Now.Day;
        }


        public static void writeCSVFile(State BotState, string deskTopFolderName, string filename, List<String> data)
        {
            if(!System.IO.Directory.Exists("C:\\Users\\alist\\Desktop\\" + deskTopFolderName))
                System.IO.Directory.CreateDirectory("C:\\Users\\alist\\Desktop\\" + deskTopFolderName + "\\");

            System.IO.File.WriteAllLines("C:\\Users\\alist\\Desktop\\" + deskTopFolderName + "\\" + filename + BotState.getMarketName() + " -" + BotState.BotId + "-" + "swordfish-" + getTimeStamp(true) + ".csv", data.ToArray());

        }


    }
}
