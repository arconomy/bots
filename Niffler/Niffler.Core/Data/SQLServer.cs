using System;
using System.Collections;
using System.Collections.Generic;
using System.Data;
using System.Diagnostics;
using System.Data.SqlClient;

namespace Niffler.Data
{

    public class SQLServer
    {


        public static IData Query( int CommandTimeOutInSeconds = 240)
        {
             
            return new MicrosoftSQLServer(Niffler.Constants.DatabaseConnectionString, CommandTimeOutInSeconds);

        }

        public enum CommandResponse
        {
            OK,
            Error,
            Inserted,
            Updated,
            Deleted,
            Other
        }

        public interface IData
        {

            bool Execute(System.Data.CommandType CommandType, string CommandText, Hashtable Parameters = null);

            t Scalar<t>(System.Data.CommandType CommandType, string CommandText, Hashtable Parameters = null);

            t Singular<t>(System.Data.CommandType CommandType, string CommandText, Hashtable Parameters = null);

            List<t> Retrieve<t>(System.Data.CommandType CommandType, string CommandText, Hashtable Parameters = null);

            DataSet Download(System.Data.CommandType CommandType, string CommandText, Hashtable Parameters = null);

            bool Upload(string TableNameAndSchema, DataTable Datatable, int BulkCopyTimeOut = 120000, int BatchSize = 10000);

        }


        private class MicrosoftSQLServer : IData
        {

            private int _MaxRetries = 3;
            private int _RetrySleepInMilliseconds = 1000;
            private string _ConnectionString = "";

            private int _CommandTimeoutInSeconds = 240;
            private bool ReasonToRetryQuery(int ExceptionNumber)
            {

                switch (ExceptionNumber)
                {

                    case 121:
                        //Semaphore Timeout Error Occured, Try again

                        return true;
                    case 64:
                        //A transport-level error has occurred when receiving results from the server. (provider: TCP Provider, error: 0 - The specified network name is no longer available.)

                        return true;
                    case 11004:
                        //General Network Error

                        return true;
                    case 40613:
                        //Database not available, contact support or try again (normally supplied with a Trace ID)

                        return true;
                    case 10054:
                        //A connection was successfully established with the server, but then an error occurred during the login process. (provider: TCP Provider, error: 0 - An existing connection was forcibly closed by the remote host.)

                        return true;
                    case 10928:
                        //resource limit reached... 
                        return true;
                    case 10060:
                    case 11001:
                        //A network-related or instance-specific error occurred while establishing a connection to SQL Server. The server was not found or was not accessible. Verify that the instance name is correct and that SQL Server is configured to allow remote connections. (provider: TCP Provider, error: 0 - A connection attempt failed because the connected party did not properly respond after a period of time, or established connection failed because connected host has failed to respond.)
                        return true;
                    case 258:
                        //A network-related or instance-specific error occurred while establishing a connection to SQL Server. The server was not found or was not accessible. Verify that the instance name is correct and that SQL Server is configured to allow remote connections. (provider: TCP Provider, error: 0 - The wait operation timed out.)
                        return true;
                    case 1205:
                        return true;
                    //deadlock victum 
                    case -2:
                        return true; 
                    case -2146893056:
                        return true;
                    default:
                        // Not sure what the error is, throw exception

                        return false;

                }
            }

            public MicrosoftSQLServer(string ConnectionString, int CommandTimeOutInSeconds)
            {
                _ConnectionString = ConnectionString;
                _CommandTimeoutInSeconds = CommandTimeOutInSeconds;
            }

            public bool Execute(CommandType CommandType, string CommandText, Hashtable Parameters = null)
            {

                using (System.Data.SqlClient.SqlConnection SqlConn = new SqlConnection(_ConnectionString))
                {
                    using (SqlCommand Sqlcommand = new SqlCommand(CommandText, SqlConn))
                    {

                        Sqlcommand.CommandType = CommandType;
                        Sqlcommand.CommandTimeout = _CommandTimeoutInSeconds;
                        if (Parameters != null)
                        {
                            foreach (string Key in Parameters.Keys)
                            {
                                Sqlcommand.Parameters.AddWithValue("@" + Key, Parameters[Key]);
                            }
                        }


                        for (int iRetry = 0; iRetry <= _MaxRetries; iRetry++)
                        {
                            try
                            {
                                SqlConn.Open();
                                Sqlcommand.ExecuteNonQuery();
                                return true;


                            }
                            catch (SqlException exsql)
                            {

                                if (!ReasonToRetryQuery(exsql.Number))
                                {
                                    throw new Exception(exsql.Message, exsql);
                                }

                            }
                            finally
                            {
                                SqlConn.Close();

                            }
                            System.Threading.Thread.Sleep(_RetrySleepInMilliseconds);
                        }

                    }
                }

                return false;


            }

            public t Scalar<t>(CommandType CommandType, string CommandText, Hashtable Parameters = null)
            {

                using (System.Data.SqlClient.SqlConnection Conn = new SqlConnection(_ConnectionString))
                {
                    using (SqlCommand sqlcommand = new SqlCommand(CommandText, Conn))
                    {
                        sqlcommand.CommandTimeout = _CommandTimeoutInSeconds;
                        sqlcommand.CommandType = CommandType;
                        if (Parameters != null)
                        {
                            foreach (string Key in Parameters.Keys)
                            {
                                sqlcommand.Parameters.AddWithValue("@" + Key, Parameters[Key]);
                            }
                        }


                        for (int iRetry = 0; iRetry <= _MaxRetries; iRetry++)
                        {
                            try
                            {
                                Conn.Open();
                                return (t)sqlcommand.ExecuteScalar();


                            }
                            catch (SqlException exsql)
                            {
                                if (!ReasonToRetryQuery(exsql.Number))
                                {
                                    throw new Exception(exsql.Message, exsql);
                                }

                            }
                            finally
                            {
                                Conn.Close();
                            }

                            System.Threading.Thread.Sleep(_RetrySleepInMilliseconds);
                        }

                    }
                }

                return default(t);


            }

            public t Singular<t>(CommandType CommandType, string CommandText, Hashtable Parameters = null)
            {

                DataSet ds = Download(CommandType, CommandText, Parameters);

                if (ds.Tables.Count > 0)
                {
                    if (ds.Tables[0].Rows.Count > 0)
                    {
                        return RowToObject<t>(ds.Tables[0].Rows[0]);
                    }
                }

                return default(t);

            }

            public List<t> Retrieve<t>(CommandType CommandType, string CommandText, Hashtable Parameters = null)
            {

                DataSet ds = Download(CommandType, CommandText, Parameters);

                System.Collections.Generic.List<t> Collection = new System.Collections.Generic.List<t>();
                if (ds.Tables.Count > 0)
                {
                    foreach (DataRow dr in ds.Tables[0].Rows)
                    {
                        Collection.Add(RowToObject<t>(dr));
                    }
                }

                return Collection;


            }

            public DataSet Download(CommandType CommandType, string CommandText, Hashtable Parameters = null)
            {

                using (System.Data.SqlClient.SqlConnection Conn = new SqlConnection(_ConnectionString))
                {
                    using (SqlDataAdapter UserAdapter = new SqlDataAdapter(CommandText, Conn))
                    {
                        UserAdapter.SelectCommand.CommandTimeout = _CommandTimeoutInSeconds;
                        UserAdapter.SelectCommand.CommandType = CommandType;

                        if (Parameters != null)
                        {
                            foreach (string Key in Parameters.Keys)
                            {
                                UserAdapter.SelectCommand.Parameters.AddWithValue("@" + Key, Parameters[Key]);
                            }
                        }


                        for (int iRetry = 0; iRetry <= _MaxRetries; iRetry++)
                        {
                                     
                            try
                            {
                                //some possible issues that could occur?
                                //http://stackoverflow.com/questions/250713/sqldataadapter-fill-method-slow 
                                //http://dba.stackexchange.com/questions/39689/how-can-i-remove-a-bad-execution-plan-from-azure-sql-database

                                DataSet ds = new DataSet();
                                UserAdapter.Fill(ds, "0");
                                return ds;


                            }
                            catch (SqlException exsql)
                            {
                                if (!ReasonToRetryQuery(exsql.Number))
                                {
                                    throw new Exception(exsql.Message, exsql);
                                }

                            }
                            catch (Exception ex) 
                            {
                                Debugger.Log(1,"Niffler.Data.SQLServer",ex.ToString());
                                UserAdapter.Dispose();
                                Conn.Dispose();
                            }

                            System.Threading.Thread.Sleep(_RetrySleepInMilliseconds);
                        }
                    }
                }

                return new DataSet();

            }

            public bool Upload(string TableNameAndSchema, DataTable Datatable, int BulkCopyTimeOut = 120000, int BatchSize = 10000)
            {

                using (SqlBulkCopy bulkCopy = new SqlBulkCopy(_ConnectionString))
                {


                    if (Datatable.Rows.Count > 0)
                    {
                        bulkCopy.DestinationTableName = TableNameAndSchema;

                        bulkCopy.BulkCopyTimeout = (BulkCopyTimeOut);

                        bulkCopy.BatchSize = BatchSize;

                        bulkCopy.WriteToServer(Datatable);

                    }

                }

                return true;

            }

            private t RowToObject<t>(DataRow Dr)
            {

                t X = Activator.CreateInstance<t>();

                foreach (DataColumn col in Dr.Table.Columns)
                {
                    foreach (System.Reflection.PropertyInfo Property in X.GetType().GetProperties())
                    {
                        if (col.ColumnName == Property.Name & Property.CanWrite)
                        {
                            if (Property.GetValue(X, null) != null | !object.ReferenceEquals(Property.GetValue(X, null), DBNull.Value))
                            {
                                try
                                {
                                    if (object.ReferenceEquals(Dr[Property.Name], DBNull.Value))
                                    {
                                        //do nothing for   dbnull
                                        Property.SetValue(X, null, null);


                                    }
                                    else if (Property.PropertyType.Name == "TimeSpan")
                                    {
                                        if (TimeSpan.TryParse(Dr[Property.Name].ToString(), out TimeSpan ts))
                                        {
                                            Property.SetValue(X, ts, null);
                                        }
                                    }
                                    else if (Property.PropertyType.IsEnum)
                                    {
                                        Property.SetValue(X, System.Enum.Parse(Property.PropertyType, Dr[Property.Name].ToString(), true), null);

                                    }
                                    else
                                    {
                                        Property.SetValue(X, Convert.ChangeType(Dr[Property.Name], Property.PropertyType), null);
                                    }

                                }
                                catch (Exception ex)
                                {
                                    Debugger.Log(1, "Niffler.Data.SQLServer", ex.ToString());
                                }
                            }
                        }

                    }
                }

                return X;

            }

        }

    }

}