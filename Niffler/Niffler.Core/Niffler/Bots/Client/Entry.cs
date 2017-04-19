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

         
        public static void Test()
        {

            //if (!Niffler.Data.Queues.Queue("test").Exists())
            //    Niffler.Data.Queues.Queue("test").Create();

            //Niffler.Model.KeyLevel K = new Niffler.Model.KeyLevel();

            //K.High = 100;


            //Niffler.Data.Queues.Queue("test").Send(K);



            //// Wait to recieve Key Level off Queue... 
            //while (true)
            //{

            //    try
            //    {
            //        Dictionary<string, object> Properties = null;
            //        dynamic RecievedKeyLevel = Niffler.Data.Queues.Queue("test").Receive<Niffler.Model.KeyLevel>(ref Properties);


            //        // Fire off Task without waiting... 
            //        //Task.Factory.StartNew(((Niffler.Model.KeyLevel)K) =>
            //        //{
            //        //    int x = 1;


            //        //}, RecievedKeyLevel);




            //    }
            //    catch (Exception ex)
            //    {


            //    }

            //}



        }

        public static void UpdateAccount(IAccount Account)
        {
             
            // Create or Update User Account.
            Niffler.Business.Accounts.Save(Account);

        }


        public static void UpdatePosition(IAccount Account, Position Position, string Status)
        {
            // Create or Update the Users Position
            Business.Positions.Save(Account, Position, Status);

        }
        public static void UpdateAccountAndPositions(IAccount Account, Positions Positions)
        {

            // Update User Account.
            Niffler.Business.Accounts.Save(Account);


            // Check for any open Positions that are no longer open in cAlgo... 
            List<Position> CurrentOpenPositions = Business.Positions.GetByAccountID(Account.Number);

            foreach (Position P in CurrentOpenPositions)
            {

                bool ClosedExternally = true;
                foreach (Position AlgoP in Positions)
                {
                    if (P.Id == AlgoP.Id) ClosedExternally = false;

                }

                if (ClosedExternally)
                {

                    Business.Positions.Save(Account, P, "Closed");
                    //AB: SEND TO SERVICE BUS
                }

            }



            // For each Open Position, update the SL/TP etc and add it if its not in the Database... 
            foreach (Position AlgoP in Positions)
            {

                var DBPosition = Business.Positions.GetByID(AlgoP.Id);

                if (DBPosition == null)
                {
                    //add it in and send out an event to SB... 
                    Business.Positions.Save(Account, DBPosition, "Opened");

                    //AB: SEND TO SERVICE BUS

                }
                else
                {

                    Boolean Difference = false;
                    foreach (PropertyInfo AlgoPosition in AlgoP.GetType().GetProperties())
                    {
                        foreach (PropertyInfo DatabasePosition in DBPosition.GetType().GetProperties())
                        {
                            // Align the Properties
                            if (AlgoPosition.Name == DatabasePosition.Name)
                            {
                                // Check if they are not the same... 
                                if (AlgoPosition.GetValue(AlgoP,null) != DatabasePosition.GetValue(DBPosition, null))
                                {
                                    Difference = true;

                                }
                            }

                        }

                    }


                    if (Difference)
                    {

                        Business.Positions.Save(Account, AlgoP, "Updated");

                        //AB: SEND TO SERVICE BUS
                    }


                }


            }



        }


        public static void UpdateKeyLevels(IAccount Account, Symbol Symbol, MarketSeries MarketSeries)
        {
            // Collect Key Levels when a Market Closes
            Indicators.KeyLevels.Main.Collect(Account, Symbol, MarketSeries);
        }

    }
}
