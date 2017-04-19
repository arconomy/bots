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

    public class Positions
    {
         
        private static string StoredProcedure = "[Operations].[Positions]";

        public static bool Save(IAccount Account, Position Position, string Status)
        {

            Hashtable Parameters = new Hashtable();
            Parameters.Add("Operation", "Save");
            Parameters.Add("ID", Position.Id);
            Parameters.Add("AccountID", Account.Number);
            Parameters.Add("Symbol", Position.SymbolCode);
            Parameters.Add("TradeType", Position.TradeType.ToString());
            Parameters.Add("Volume", Position.Volume);
            Parameters.Add("Quantity", Position.Quantity);
            Parameters.Add("EntryPrice", Position.EntryPrice);
            Parameters.Add("StopLoss", Position.StopLoss);
            Parameters.Add("TakeProfit", Position.TakeProfit);
            Parameters.Add("ClosedPrice", "");
            Parameters.Add("Swap", Position.Swap);
            Parameters.Add("Channel", "");
            Parameters.Add("Label", Position.Label);
            Parameters.Add("Comment", Position.Comment);
            Parameters.Add("GrossProfit", Position.GrossProfit);
            Parameters.Add("NetProfit", Position.NetProfit);
            Parameters.Add("Pips", Position.Pips);
            if (Status == "Opened")
                Parameters.Add("CreatedUTC", DateTime.UtcNow);

            Parameters.Add("LastModifiedUTC", DateTime.UtcNow);
            Parameters.Add("Status", Status);

            return Data.SQLServer.Query().Execute(CommandType.StoredProcedure, StoredProcedure, Parameters);

        }

        public static Position GetByID(int ID)
        {
            return General.Query(StoredProcedure).GetByID<Position>(ID);
        }

        public static List<Position> GetByAccountID(int AccountID)
        {
            Hashtable Parameters = new Hashtable();
            Parameters.Add("Operation", "GetByAccountID"); 
            Parameters.Add("AccountID", AccountID);
            

            return Data.SQLServer.Query().Retrieve<Position >(CommandType.StoredProcedure, StoredProcedure, Parameters);

        }


        public static List<Position> GetbyStatus(string Status)
        {
            return General.Query(StoredProcedure).GetByStatus<Position>(Status);
        }

    }

}