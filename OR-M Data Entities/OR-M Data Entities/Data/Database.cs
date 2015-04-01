/*
 * OR-M Data Entities v1.2.0
 * License: The MIT License (MIT)
 * Code: https://github.com/jdemeuse1204/OR-M-Data-Entities
 * (c) 2015 James Demeuse
 */

using System;
using System.Configuration;
using System.Data;
using System.Data.SqlClient;
using OR_M_Data_Entities.Expressions;

namespace OR_M_Data_Entities.Data
{
    public abstract class Database : ExpressionResolver, IDisposable
    {
        protected string ConnectionString { get; private set; }
        protected SqlConnection Connection { get; set; }
        protected SqlCommand Command { get; set; }
        protected PeekDataReader Reader { get; set; }

        protected Database(string connectionStringOrName)
        {
            if (connectionStringOrName.Contains(";") || connectionStringOrName.Contains("="))
            {
                ConnectionString = connectionStringOrName;
            }
            else
            {
                var conn = ConfigurationManager.ConnectionStrings[connectionStringOrName];

                if (conn == null) throw new ConfigurationErrorsException("Connection string not found in config");

                ConnectionString = conn.ConnectionString;
            }
            
            Connection = new SqlConnection(ConnectionString);
        }

        /// <summary>
        /// Connect our SqlConnection
        /// </summary>
        protected void Connect()
        {
            // Open the connection if its closed
            if (Connection.State == ConnectionState.Closed)
            {
                Connection.Open();
            }
        }

        /// <summary>
        /// Disconnect the SqlConnection
        /// </summary>
        public void Disconnect()
        {
            // Disconnect our connection
            Connection.Close();
        }

        public void Dispose()
        {
            // disconnect our db reader
            if (Reader != null)
            {
                Reader.Close();
                Reader.Dispose();
            }

            // dispose of our sql command
            if (Command != null)
            {
                Command.Dispose();
            }

            // close our connection
            if (Connection != null)
            {
                Connection.Close();
                Connection.Dispose();
            }
        }
    }
}
