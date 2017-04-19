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


            Niffler.Bots.Client.Entry.Test();

            Niffler.Bots.Client.Entry.UpdateAccount(null);

            Niffler.Bots.Client.Entry.UpdatePosition(null, null,null);

            Niffler.Bots.Client.Entry.UpdateAccountAndPositions(null,null);
             


        }
    }
}
