using System;
using System.Collections;
using System.Collections.Generic;
using System.Data;

namespace Niffler.Data
{

    public class General
    {

        public static ICommon Query(string CommandText)
        {
            return new Common(CommandText);
        }

        public enum StatusEnum
        {
            Inserted,
            Updated,
            Deleted,
            Errored
        }

        public interface ICommon
        {

            bool Save<t>(t Item);
            StatusEnum SaveWithStatus<t>(t Item);
            t GetByID<t>(Guid ID);
            t GetByID<t>(int ID);
            t GetByID<t>(string ID);
            t GetByID<t>(string PartitionKey, string RowKey);
            List<t> GetByObject<t>(t Item);
            List<t> GetByParentID<t>(Guid ID);
            List<t> GetByParentID<t>(string ID);
            List<t> GetByStatus<t>(string Status);
            List<t> GetAll<t>();
            List<t> GetAll<t>(string PartitionKey);
            List<t> GetList<t>();
            List<t> GetHash<t>(string HashVersion = "");
            bool Delete(Guid ID);
            bool Delete(string ID);
            bool Delete(string PartitionKey, string RowKey);
        }

        public class Common : ICommon
        {
            private string _CommandText = "";
            public Common(string CommandText)
            {
                _CommandText = CommandText;
            }

            public bool Save<t>(t Item)
            {
                return  SQLServer.Query().Execute(CommandType.StoredProcedure, _CommandText, Niffler.Data.Objects.ToHashTable<t>(Item, "Save"));
            }

            public StatusEnum SaveWithStatus<t>(t Item)
            {
                return StatusEnum.Errored;
              //  string Status = SQLServer.Query().Scalar<string>(CommandType.StoredProcedure, _CommandText, BottyTest.Common.Object.ToHashTable<t>(Item, "Save"));

             //   return System.Enum.Parse(typeof(StatusEnum), Status);

            }

            public t GetByID<t>(Guid ID)
            {
                Hashtable Parameters = new Hashtable
                {
                    { "Operation", "GetByID" },
                    { "ID", ID }
                };
                return SQLServer.Query().Singular<t>(CommandType.StoredProcedure, _CommandText, Parameters);
            }

            public t GetByID<t>(string ID)
            {
                Hashtable Parameters = new Hashtable
                {
                    { "Operation", "GetByID" },
                    { "ID", ID }
                };
                return SQLServer.Query().Singular<t>(CommandType.StoredProcedure, _CommandText, Parameters);
            }

            public t GetByID<t>(int ID)
            {
                Hashtable Parameters = new Hashtable
                {
                    { "Operation", "GetByID" },
                    { "ID", ID }
                };
                return SQLServer.Query().Singular<t>(CommandType.StoredProcedure, _CommandText, Parameters);
            }

            public t GetByID<t>(string PartitionKey, string RowKey)
            {
                Hashtable Parameters = new Hashtable
                {
                    { "Operation", "GetByID" },
                    { "PartitionKey", PartitionKey },
                    { "RowKey", RowKey }
                };
                return SQLServer.Query().Singular<t>(CommandType.StoredProcedure, _CommandText, Parameters);
            }

            public List<t> GetByObject<t>(t Item)
            {
                return SQLServer.Query().Retrieve<t>(CommandType.StoredProcedure, _CommandText, Niffler.Data.Objects.ToHashTable<t>(Item, "GetByObject"));
            }

            public List<t> GetByStatus<t>(string Status)
            {
                Hashtable Parameters = new Hashtable
                {
                    { "Operation", "GetByStatus" },
                    { "Status", Status }
                };
                return SQLServer.Query().Retrieve<t>(CommandType.StoredProcedure, _CommandText, Parameters);
            }

            public List<t> GetAll<t>()
            {
                Hashtable Parameters = new Hashtable
                {
                    { "Operation", "GetAll" }
                };
                return SQLServer.Query().Retrieve<t>(CommandType.StoredProcedure, _CommandText, Parameters);
            }

            public List<t> GetAll<t>(string PartitionKey)
            {
                Hashtable Parameters = new Hashtable
                {
                    { "Operation", "GetAll" },
                    { "PartitionKey", PartitionKey }
                };
                return SQLServer.Query().Retrieve<t>(CommandType.StoredProcedure, _CommandText, Parameters);
            }

            public List<t> GetList<t>()
            {
                Hashtable Parameters = new Hashtable
                {
                    { "Operation", "GetList" }
                };
                return SQLServer.Query().Retrieve<t>(CommandType.StoredProcedure, _CommandText, Parameters);
            }

            public List<t> GetHash<t>(string HashVersion = "")
            {
                Hashtable Parameters = new Hashtable
                {
                    { "Operation", "GetHash" + (!string.IsNullOrEmpty(HashVersion) ? "-" + HashVersion : "") }
                };
                return SQLServer.Query().Retrieve<t>(CommandType.StoredProcedure, _CommandText, Parameters);
            }

            public bool Delete(Guid ID)
            {
                Hashtable Parameters = new Hashtable
                {
                    { "Operation", "Delete" },
                    { "ID", ID }
                };
                return SQLServer.Query().Execute(CommandType.StoredProcedure, _CommandText, Parameters);
            }

            public bool Delete(string ID)
            {
                Hashtable Parameters = new Hashtable
                {
                    { "Operation", "Delete" },
                    { "ID", ID }
                };
                return SQLServer.Query().Execute(CommandType.StoredProcedure, _CommandText, Parameters);
            }

            public bool Delete(string PartitionKey, string RowKey)
            {
                Hashtable Parameters = new Hashtable
                {
                    { "Operation", "Delete" },
                    { "PartitionKey", PartitionKey },
                    { "RowKey", RowKey }
                };
                return SQLServer.Query().Execute(CommandType.StoredProcedure, _CommandText, Parameters);
            }

            public List<t> GetByParentID<t>(Guid ParentID)
            {
                Hashtable Parameters = new Hashtable
                {
                    { "Operation", "GetByParentID" },
                    { "ParentID", ParentID }
                };
                return SQLServer.Query().Retrieve<t>(CommandType.StoredProcedure, _CommandText, Parameters);
            }

            public List<t> GetByParentID<t>(string ParentID)
            {
                Hashtable Parameters = new Hashtable
                {
                    { "Operation", "GetByParentID" },
                    { "ParentID", ParentID }
                };
                return SQLServer.Query().Retrieve<t>(CommandType.StoredProcedure, _CommandText, Parameters);
            }
        }

    }

}
