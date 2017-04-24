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


        public static void UpdatePositions(IAccount Account, cAlgo.API.Positions Positions )
        {
             
            // Check for any open Positions that are no longer open in cTrader... 
            List<Model.Position> CurrentOpenPositions = Business.Positions.GetByAccountID(Account.Number, "Opened");

            foreach (Model.Position LocalPosition in CurrentOpenPositions)
            {

                bool ClosedExternally = true;
                foreach (cAlgo.API.Position AlgoP in Positions)
                    if (LocalPosition.ID == AlgoP.Id) ClosedExternally = false;

                if (ClosedExternally)
                {
                    LocalPosition.Status = "Closed";
                    Business.Positions.Save(LocalPosition);
                    //AB: SEND TO SERVICE BUS to CLOSE Positions...
                }

            }



            // For each Open Position, update the SL/TP etc and add it if its not in the Database... 
            foreach (cAlgo.API.Position AlgoP in Positions)
            {

                Model.Position LocalPosition = Business.Positions.GetByID(AlgoP.Id);

                // find any changes to syn out to shadows...
                //if (AlgoP.TakeProfit <> LocalPosition.TakeProfit | AlgoP.StopLoss  <> LocalPosition.StopLoss )

                //Update Postions...
                 Update(Account, AlgoP, "Opened");


            }



        }



        public static bool Update(IAccount Account, cAlgo.API.Position Position, string Status)
        {

            Model.Position LocalPosition = GetByID(Position.Id);

            if (LocalPosition is null) LocalPosition = new Model.Position();

            LocalPosition.ID = Position.Id;
            LocalPosition.AccountID = Account.Number;
            LocalPosition.Symbol = Position.SymbolCode;
            LocalPosition.TradeType = Position.TradeType.ToString();
            if (Position.Volume != 0) LocalPosition.Volume = Position.Volume;
            if (Position.Quantity != 0) LocalPosition.Quantity = Position.Quantity;
            if (Position.EntryPrice != 0) LocalPosition.EntryPrice = (decimal)Position.EntryPrice;
            if (Position.StopLoss != null) LocalPosition.StopLoss = (decimal)Position.StopLoss;
            if (Position.TakeProfit != null) LocalPosition.TakeProfit = (decimal)Position.TakeProfit;
            // LocalPosition.ClosedPrice = "";
            if (Position.Swap != 0) LocalPosition.Swap = (decimal)Position.Swap;
            LocalPosition.Channel = "";
            if (Position.Label != null) LocalPosition.Label = Position.Label;
            if (Position.Comment != null) LocalPosition.Comment = Position.Comment;
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