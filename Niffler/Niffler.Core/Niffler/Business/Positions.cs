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

    public class Positions
    {

        private static string StoredProcedure = "[Operations].[Positions]";

        public enum StatusEnum
        {
            Open,
            Closed
        }

        public static bool Update(IAccount Account, cAlgo.API.Position Position, string Status)
        {

            Model.Position LocalPosition = GetByID(Position.Id);

            if (LocalPosition is null) LocalPosition = new Model.Position();

            LocalPosition.ID = Position.Id;
            LocalPosition.AccountID = Account.Number;
            LocalPosition.Symbol = Position.SymbolCode;
            LocalPosition.TradeType = Position.TradeType.ToString();
            LocalPosition.Volume = Position.Volume;
            LocalPosition.Quantity = Position.Quantity;
            LocalPosition.EntryPrice = (decimal)Position.EntryPrice;
            if (Position.StopLoss != null) LocalPosition.StopLoss = (decimal)Position.StopLoss;
            if (Position.TakeProfit != null) LocalPosition.TakeProfit = (decimal)Position.TakeProfit;
            // LocalPosition.ClosedPrice = "";
            LocalPosition.Swap = (decimal)Position.Swap;
            LocalPosition.Channel = "";
            LocalPosition.Label = Position.Label;
            LocalPosition.Comment = Position.Comment;
            LocalPosition.GrossProfit = (decimal)Position.GrossProfit;
            LocalPosition.NetProfit = (decimal)Position.NetProfit;
            LocalPosition.Pips = Position.Pips;

            if (Status == "Opened")
                LocalPosition.DateTimeCreatedUTC = DateTime.UtcNow;

            LocalPosition.DateTimeLastModifiedUTC = DateTime.UtcNow;
            LocalPosition.Status = Status;

            return Save(LocalPosition);

        }


        public enum ChangedEnum
        {
            StopLoss,
            TakeProfit,
            Volume,
            Status,
            Unknown
        }

        public static ChangedEnum WhatsChanged(cAlgo.API.Position Position)
        {

            Model.Position LocalPosition = GetByID(Position.Id);


            if (LocalPosition == null)
                return ChangedEnum.Status;
            // else if (LocalPosition.TakeProfit <> (decimal)Position.TakeProfit  )
            //   return ChangedEnum.TakeProfit 

            return ChangedEnum.Unknown;
        }






        public static bool Save(Model.Position Position)
        {

            Hashtable Parameters = Data.Objects.ToHashTable(Position, "Save");

            return Data.SQLServer.Query().Execute(CommandType.StoredProcedure, StoredProcedure, Parameters);

        }

        public static Model.Position GetByID(int ID)
        {
            return General.Query(StoredProcedure).GetByID<Model.Position>(ID);
        }

        public static List<Model.Position> GetByAccountID(int AccountID, string Status)
        {
            Hashtable Parameters = new Hashtable();
            Parameters.Add("Operation", "GetByAccountID");
            Parameters.Add("AccountID", AccountID);
            Parameters.Add("Status", Status);

            return Data.SQLServer.Query().Retrieve<Model.Position>(CommandType.StoredProcedure, StoredProcedure, Parameters);

        }


        public static List<Model.Position> GetbyStatus(string Status)
        {
            return General.Query(StoredProcedure).GetByStatus<Model.Position>(Status);
        }

    }

}