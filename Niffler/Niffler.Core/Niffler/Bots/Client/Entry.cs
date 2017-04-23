using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using cAlgo.API;
using cAlgo.API.Indicators;
using cAlgo.API.Internals;
using cAlgo.Indicators;
using System.Reflection;

namespace Niffler.Bots.Client
{
    public class Entry
    {

        public static double LastPrice;

     //   Niffler.Bots.Client.Entry.TestOrder(Symbol);
            public static void TestOrder(Symbol Symbol, Robot Robot)
        {

            // how to send buys and sells to cAlgo from here.....
            // https://ctdn.com/algos/cbots/show/223

         //   Robot.PlaceLimitOrder(TradeType.Buy, Symbol, 1, 7200);

            
            

        }


        public static void OnTick(Symbol Symbol,Positions P)
        {


            Niffler.Rules.Test.Data = Symbol.Ask;

            if (Niffler.Rules.Test.AreAllPositionsClosed (P) == true)
            {
                var d = 3;
            }

            if (Niffler.Rules.Test.Rule2() == true)
            {
                var d = 3;
            }
             

        }

            public static void Test()
        {
             
           


          //  if (!Niffler.Data.Queues.Queue("test").Exists())
            //    Niffler.Data.Queues.Queue("test").Create();

            Niffler.Model.KeyLevel K = new Niffler.Model.KeyLevel();

            K.High = 100;
             
            Niffler.Data.Queues.Queue("test").Send(K);



            // Wait to recieve Key Level off Queue... 
            while (true)
            {

                try
                {
                    Dictionary<string, object> Properties = null;
                    dynamic RecievedKeyLevel = Niffler.Data.Queues.Queue("test").Receive<Niffler.Model.KeyLevel>(ref Properties);


                     
                 //Task(0) =   Task.Factory.StartNew(((Niffler.Model.KeyLevel)K) =>
                 //   {
                 //       int x = 1;


                 //   }, RecievedKeyLevel);

                 //   System.Threading.Tasks.Task.WaitAll(task);



                }
                catch (Exception ex)
                {


                }

            }



        }


        public static void UpdatePosition(IAccount Account, Position Position, string Status)
        {
            // Create or Update the Users Position
            Business.Positions.Update(Account, Position, Status);

        }
        public static void UpdateAccountAndPositions(IAccount Account, Positions Positions)
        {

            // Update User Account.
            Niffler.Business.Accounts.Update(Account);


            // Check for any open Positions that are no longer open in cTrader... 
            List<Model.Position> CurrentOpenPositions = Business.Positions.GetByAccountID(Account.Number, "Opened");

            foreach (Model.Position LocalPosition in CurrentOpenPositions)
            {

                bool ClosedExternally = true;
                foreach (Position AlgoP in Positions)
                    if (LocalPosition.ID == AlgoP.Id) ClosedExternally = false;

                if (ClosedExternally)
                {
                    LocalPosition.Status = "Closed";
                    Business.Positions.Save(LocalPosition);
                    //AB: SEND TO SERVICE BUS to CLOSE Positions...
                }

            }



            // For each Open Position, update the SL/TP etc and add it if its not in the Database... 
            foreach (Position AlgoP in Positions)
            {

                Model.Position LocalPosition = Business.Positions.GetByID(AlgoP.Id);

                // find any changes to syn out to shadows...
                //if (AlgoP.TakeProfit <> LocalPosition.TakeProfit | AlgoP.StopLoss  <> LocalPosition.StopLoss )
                 
                //Update Postions...
                Business.Positions.Update(Account, AlgoP, "Opened");
                 

            }



        }


        public static void UpdateKeyLevels(IAccount Account, Symbol Symbol, MarketSeries MarketSeries)
        {
            // Collect Key Levels when a Market Closes
            Indicators.KeyLevels.Main.Collect(Account, Symbol, MarketSeries);
        }

    }
}
