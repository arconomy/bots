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
using Niffler.Data;

namespace Niffler.Business
{

    public class Accounts
    {
         
        private static string StoredProcedure = "[Operations].[Accounts]";

        public static bool Save(IAccount Account)
        {

            Hashtable Parameters = new Hashtable();
            Parameters.Add("Operation", "Save");
            Parameters.Add("ID", Account.Number);
            //  Parameters.Add("SlackID", "")
            //   Parameters.Add("Name", "")
            Parameters.Add("Balance", Account.Balance);
            Parameters.Add("Currency", Account.Currency);
            Parameters.Add("BrokerName", Account.BrokerName);
            Parameters.Add("Equity", Account.Equity);
            Parameters.Add("IsLive", Account.IsLive);
            Parameters.Add("Margin", Account.Margin);
            Parameters.Add("MarginLevel", Account.MarginLevel);
            Parameters.Add("PreciseLeverage", Account.PreciseLeverage);
            Parameters.Add("UnrealizedGrossProfit", Account.UnrealizedGrossProfit);
            Parameters.Add("UnrealizedNetProfit", Account.UnrealizedNetProfit);
            Parameters.Add("DateTimeLastModifiedUTC", DateTime.UtcNow);
            //     Parameters.Add("Status", "")

            return Data.SQLServer.Query().Execute(CommandType.StoredProcedure, StoredProcedure, Parameters);

        }

        public static IAccount GetByID(int ID)
        {
            return General.Query(StoredProcedure).GetByID<IAccount>(ID);
        }

        public static List<IAccount> GetByStatus(string Status)
        {
            return General.Query(StoredProcedure).GetByStatus<IAccount>(Status);
        }
         

    }

}