
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

        public static DataSet GetReport(string BrokerName, string Symbol, DateTime Date)
        {

            Hashtable Parameters = new Hashtable();
            Parameters.Add("Operation", "GetReport");
            Parameters.Add("BrokerName", BrokerName);
            Parameters.Add("Symbol", Symbol);
            Parameters.Add("Date", Date);

            return Data.SQLServer.Query().Download(CommandType.StoredProcedure, StoredProcedure, Parameters);

        }
    }

}