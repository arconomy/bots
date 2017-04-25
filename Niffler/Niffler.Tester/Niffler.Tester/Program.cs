using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Niffler.Tester
{
    class Program
    {
        static void Main(string[] args)
        {


        Niffler.Model.KeyLevel     YesterdayKeyLevels = Business.KeyLevels.GetYesterdaysKeyLevels("IC Markets", "UK100");

            YesterdayKeyLevels = YesterdayKeyLevels;
             
        }
    }
}
