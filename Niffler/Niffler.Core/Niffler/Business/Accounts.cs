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
using Niffler.Model;

namespace Niffler.Business
{

    public class Accounts
    {

        private static string StoredProcedure = "[Operations].[Accounts]";

        public static bool Update(IAccount Account)
        {

            Model.Account LocalAccount = GetByID(Account.Number);

            if (LocalAccount is null) LocalAccount = new Model.Account();

            LocalAccount.ID = Account.Number;
            LocalAccount.Balance = (decimal)Account.Balance;
            LocalAccount.Currency = Account.Currency;
            LocalAccount.BrokerName = Account.BrokerName;
            LocalAccount.Equity = (decimal)Account.Equity;
            LocalAccount.IsLive = Account.IsLive;
            LocalAccount.Margin = (decimal)Account.Margin;
            if (Account.MarginLevel != null) LocalAccount.MarginLevel = (float)Account.MarginLevel;
            LocalAccount.PreciseLeverage = (float)Account.PreciseLeverage;
            LocalAccount.UnrealizedGrossProfit = (decimal)Account.UnrealizedGrossProfit;
            LocalAccount.UnrealizedNetProfit = (decimal)Account.UnrealizedNetProfit;
            LocalAccount.DateTimeLastModifiedUTC = DateTime.UtcNow;

            return Save(LocalAccount);
        }

        public static bool Save(Model.Account Account)
        {

            Hashtable Parameters = Data.Objects.ToHashTable(Account, "Save");

            return Data.SQLServer.Query().Execute(CommandType.StoredProcedure, StoredProcedure, Parameters);

        }

        public static Model.Account GetByID(int ID)
        {
            return General.Query(StoredProcedure).GetByID<Model.Account>(ID);
        }

        public static List<Model.Account> GetByStatus(string Status)
        {
            return General.Query(StoredProcedure).GetByStatus<Model.Account>(Status);
        }


    }

}