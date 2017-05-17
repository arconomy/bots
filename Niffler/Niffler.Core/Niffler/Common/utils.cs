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
                return System.DateTime.Now.Year.ToString() + System.DateTime.Now.Month + System.DateTime.Now.Day + System.DateTime.Time.Minute + System.DateTime.Time.Second;
            return System.DateTime.Now.Year + "-" + System.DateTime.Now.Month + "-" + System.DateTime.Now.Day;
        }


        public static void writeCSVFile(string deskTopFolderName, string filename)
        {
            if(!System.IO.Directory.Exists("C:\\Users\\alist\\Desktop\\" + deskTopFolderName))
                System.IO.Directory.CreateDirectory("C:\\Users\\alist\\Desktop\\" + deskTopFolderName + "\\");


            System.IO.File.WriteAllLines("C:\\Users\\alist\\Desktop\\" + deskTopFolderName + "\\" + filename + _swordFishTimeInfo.market + " -" + _botId + "-" + "swordfish-" + getTimeStamp(true) + ".csv", debugCSV.ToArray());

        }


    }
}
