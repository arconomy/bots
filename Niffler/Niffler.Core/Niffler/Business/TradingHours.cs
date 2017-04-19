using Microsoft.VisualBasic;
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

    public class TradingHours
    {
         
        private static string StoredProcedure = "[Operations].[TradingHours]";
        public static bool Save(Model.TradingHour TradingHour)
        {

            Hashtable Parameters = Data.Objects.ToHashTable(TradingHour, "Save");
            return Data.SQLServer.Query().Execute(CommandType.StoredProcedure, StoredProcedure, Parameters);

        }

        public static Model.TradingHour GetByID(string Symbol, string Type = "Market")
        {

            Hashtable Parameters = new Hashtable();
            Parameters.Add("Operation", "GetByID");
            Parameters.Add("Symbol", Symbol);
            Parameters.Add("Type", Type);

            return Data.SQLServer.Query().Singular<Model.TradingHour>(CommandType.StoredProcedure, StoredProcedure, Parameters);
 
        }

        public enum StatusEnum
        {
            HasNotOpenedYet,
            Open,
            HasClosedForTheDay,
            NotSure
        }

        public static StatusEnum GetStatus(string Symbol, string Type = "Market")
        {

            dynamic T = GetByID(Symbol, Type);

            if (T == null)
            {
                return StatusEnum.NotSure;
            }
            else if (T.OpenTimeUTC > DateTime.UtcNow.TimeOfDay)
            {
                return StatusEnum.HasNotOpenedYet;
            }
            else if (T.OpenTimeUTC <= DateTime.UtcNow.TimeOfDay & T.CloseTimeUTC > DateTime.UtcNow.TimeOfDay)
            {
                return StatusEnum.Open;
            }
            else if (T.CloseTimeUTC >= DateTime.UtcNow.TimeOfDay)
            {
                return StatusEnum.HasClosedForTheDay;
            }
            else
            {
                return StatusEnum.NotSure;
            }

        }
    }

}
