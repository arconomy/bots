using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using cAlgo.API;
using cAlgo.API.Indicators;
using cAlgo.API.Internals;

namespace Niffler.Rules
{
    class Test
    {

        public static double Data; 

        public static bool AreAllPositionsClosed(Positions Positions)
        {

            return (Positions.Count == 0);
            //bool AreAllPositionsClosed
            //foreach (Position P in Positions)
            //{

            //    if (P.NetProfit > 10)
            //    {
            //        // CLose the positions... 
            //    }
            //}


        }


        public static bool Rule2()
        {
            return false;

        }

    }
}
