/*
 * OR-M Data Entities v2.3
 * License: The MIT License (MIT)
 * Code: https://github.com/jdemeuse1204/OR-M-Data-Entities
 * Email: james.demeuse@gmail.com
 * Copyright (c) 2015 James Demeuse
 */

using System;
using System.Configuration;
using System.Data;
using System.Data.SqlClient;

namespace OR_M_Data_Entities.Data
{
    public abstract class Database : IDisposable
    {
        #region Properties
        protected string ConnectionString { get; private set; }
        protected SqlConnection Connection { get; set; }
        protected SqlCommand Command { get; set; }
        protected PeekDataReader Reader { get; set; }
        #endregion

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
        protected bool Connect()
        {
            try
            {
                // Open the connection if its closed
                if (Connection.State == ConnectionState.Closed)
                {
                    Connection.Open();
                }
                return true;
            }
            catch (Exception)
            {
                Connection = new SqlConnection(ConnectionString);

                return false;
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

        protected void TryDisposeCloseReader()
        {
            if (Reader == null) return;

            Reader.Close();
            Reader.Dispose();
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
