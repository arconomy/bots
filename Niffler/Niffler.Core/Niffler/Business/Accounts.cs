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

        public static bool Update(IAccount Account, string Name, string Email)
        {

            Model.Account CloudAccount = GetByID(Account.Number);

            if (CloudAccount is null) CloudAccount = new Model.Account();

            CloudAccount.ID = Account.Number;
            CloudAccount.Name = Name;
            CloudAccount.Email = Email;
            CloudAccount.Balance = (decimal)Account.Balance;
            CloudAccount.Currency = Account.Currency;
            CloudAccount.BrokerName = Account.BrokerName;
            CloudAccount.Equity = (decimal)Account.Equity;
            CloudAccount.IsLive = Account.IsLive;
            CloudAccount.Margin = (decimal)Account.Margin;
            if (Account.MarginLevel != null) CloudAccount.MarginLevel = (float)Account.MarginLevel;
            CloudAccount.PreciseLeverage = (float)Account.PreciseLeverage;
            CloudAccount.UnrealizedGrossProfit = (decimal)Account.UnrealizedGrossProfit;
            CloudAccount.UnrealizedNetProfit = (decimal)Account.UnrealizedNetProfit;
            CloudAccount.DateTimeLastModifiedUTC = DateTime.UtcNow;

            return Save(CloudAccount);
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