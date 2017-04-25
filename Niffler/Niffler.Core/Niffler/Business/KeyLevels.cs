
using System;
using System.Collections;
using System.Collections.Generic;
using System.Data;
using System.Diagnostics;
using System.Linq;
using cAlgo.API;
using cAlgo.API.Indicators;
using cAlgo.API.Internals;
using cAlgo.Indicators;

namespace Niffler.Business
{

    public class KeyLevels
    {

        private static string StoredProcedure = "[Operations].[KeyLevels]";
        public static bool Save(Model.KeyLevel KeyLevel)
        {

            Hashtable Parameters = Data.Objects.ToHashTable(KeyLevel, "Save");
            return Data.SQLServer.Query().Execute(CommandType.StoredProcedure, StoredProcedure, Parameters);

        }


        public static Model.KeyLevel GetByID(string BrokerName, string Symbol, DateTime Date)
        {

            Hashtable Parameters = new Hashtable();
            Parameters.Add("Operation", "GetByID");
            Parameters.Add("BrokerName", BrokerName);
            Parameters.Add("Symbol", Symbol);
            Parameters.Add("Date", Date);

            return Data.SQLServer.Query().Singular<Model.KeyLevel>(CommandType.StoredProcedure, StoredProcedure, Parameters);

        }

        public static Model.KeyLevel GetYesterdaysKeyLevels(string BrokerName, string Symbol)
        {

            Hashtable Parameters = new Hashtable();
            Parameters.Add("Operation", "GetYesterdaysKeyLevels");
            Parameters.Add("BrokerName", BrokerName);
            Parameters.Add("Symbol", Symbol);

            return Data.SQLServer.Query().Singular<Model.KeyLevel>(CommandType.StoredProcedure, StoredProcedure, Parameters);

        }


        public static void Collect(Niffler.Indicators.KeyLevels.Bot Robot)
        {


            List<string> SymbolsToCollect = new List<string>();

            SymbolsToCollect.Add("UK100");
            SymbolsToCollect.Add("AUS200");
            SymbolsToCollect.Add("DE30");
            SymbolsToCollect.Add("F40");
            SymbolsToCollect.Add("HK50");
            SymbolsToCollect.Add("JP225");
            SymbolsToCollect.Add("US30");

             
            foreach (string StrSymbol in SymbolsToCollect)
            {
                //Get the Symbol and Market Data
                Symbol Symbol = Robot.MarketData.GetSymbol(StrSymbol);
                MarketSeries Series = Robot.MarketData.GetSeries(StrSymbol, TimeFrame.Minute30);
                Model.TradingHour TradingHours = Business.TradingHours.GetByID(StrSymbol);
               

                // Starting from Yesterday, go back a month and check we have all the data we need...

                //for (int i = 0, i < 30, i++)
                //{
                //    DateTime Date = DateTime.UtcNow.AddDays(-i);
                    
                //    //Check to see if we have the Key Level Already
                //    Model.KeyLevel KeyLevel = Business.KeyLevels.GetByID(Robot.Account.BrokerName, Symbol.Code, Series.OpenTime.LastValue.Date);

                //    // If we dont, Calculate and add it in...
                //    if (KeyLevel == null)
                //    {



                //        double High = int.MinValue;
                //        double Low = int.MaxValue;
                //        double Open = 0;
                //        double Close = 0;


                //        //24 hours in 30 minutes...
                //        int TimeSeriesBack = 0;
                //       while(true)
                //        {

                //        //    Series.OpenTime.

                //        //    if (MarketSeries.OpenTime.Last(i).Date.ToUniversalTime().TimeOfDay >= TradingHours.OpenTimeUTC & MarketSeries.OpenTime.Last(i).Date.ToUniversalTime().TimeOfDay < TradingHours.CloseTimeUTC)
                //        //    {
                //        //        if (High < MarketSeries.High.Last(i))
                //        //            High = MarketSeries.High.Last(i);

                //        //        if (Low >= MarketSeries.Low.Last(i))
                //        //            Low = MarketSeries.Low.Last(i);

                //        //        if (MarketSeries.OpenTime.Last(i).ToUniversalTime().TimeOfDay == TradingHours.OpenTimeUTC)
                //        //            Open = MarketSeries.Open.Last(i);

                //        //        if (MarketSeries.OpenTime.Last(i).ToUniversalTime().TimeOfDay == TradingHours.CloseTimeUTC)
                //        //            Close = MarketSeries.Open.Last(i);

                //        //    }

                //        //    TimeSeriesBack -= 1;
                //        //    break;

                //        //}


                //        //KeyLevel.High = (decimal)High;
                //        //KeyLevel.Low = (decimal)Low;
                //        //KeyLevel.Open = (decimal)Open;
                //        //KeyLevel.Close = (decimal)Close;
                //        //Business.KeyLevels.Save(KeyLevel);




                //    }

                //}






            }




            //if (KeyLevel == null & Business.TradingHours.GetStatus(Robot.Symbol.Code) == Business.TradingHours.StatusEnum.HasClosedForTheDay)
            //{

            //    dynamic TradingHours = Business.TradingHours.GetByID(Symbol.Code);

            //    KeyLevel = new Model.KeyLevel();
            //    KeyLevel.Symbol = Symbol.Code;
            //    KeyLevel.BrokerName = Account.BrokerName;
            //    KeyLevel.Date = MarketSeries.OpenTime.LastValue.Date;

            //    double High = int.MinValue;
            //    double Low = int.MaxValue;
            //    double Open = 0;
            //    double Close = 0;


            //    //24 hours in 30 minutes...
            //    for (int i = 0; i <= 48; i++)
            //    {


            //        if (MarketSeries.OpenTime.Last(i).Date.ToUniversalTime().TimeOfDay >= TradingHours.OpenTimeUTC & MarketSeries.OpenTime.Last(i).Date.ToUniversalTime().TimeOfDay < TradingHours.CloseTimeUTC)
            //        {
            //            if (High < MarketSeries.High.Last(i))
            //                High = MarketSeries.High.Last(i);

            //            if (Low >= MarketSeries.Low.Last(i))
            //                Low = MarketSeries.Low.Last(i);

            //            if (MarketSeries.OpenTime.Last(i).ToUniversalTime().TimeOfDay == TradingHours.OpenTimeUTC)
            //                Open = MarketSeries.Open.Last(i);

            //            if (MarketSeries.OpenTime.Last(i).ToUniversalTime().TimeOfDay == TradingHours.CloseTimeUTC)
            //                Close = MarketSeries.Open.Last(i);

            //        }

            //    }


            //    KeyLevel.High = (decimal)High;
            //    KeyLevel.Low = (decimal)Low;
            //    KeyLevel.Open = (decimal)Open;
            //    KeyLevel.Close = (decimal)Close;
            //    Business.KeyLevels.Save(KeyLevel);

            //}






        }
    }

}