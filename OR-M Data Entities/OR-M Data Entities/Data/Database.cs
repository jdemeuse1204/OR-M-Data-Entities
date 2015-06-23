/*
 * OR-M Data Entities v2.0
 * License: The MIT License (MIT)
 * Code: https://github.com/jdemeuse1204/OR-M-Data-Entities
 * Copyright (c) 2015 James Demeuse
 */

using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Data.SqlClient;
using OR_M_Data_Entities.Data.Definition;

namespace OR_M_Data_Entities.Data
{
    public abstract class Database : IDisposable
    {
        #region Properties
        protected string ConnectionString { get; private set; }
        protected SqlConnection Connection { get; set; }
        protected SqlCommand Command { get; set; }
        protected PeekDataReader Reader { get; set; }

        private string _schemaName;

        public string SchemaName
        {
            get { return string.IsNullOrWhiteSpace(_schemaName) ? "DBO" : _schemaName; }
            set { _schemaName = value; }
        }

        public bool IsLazyLoadEnabled { get; set; }

        public Dictionary<string, ObjectSchematic> SavedTableSchematics { get; private set; }
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

            SavedTableSchematics = new Dictionary<string, ObjectSchematic>();
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
