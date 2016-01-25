/*
 * OR-M Data Entities v3.0
 * License: The MIT License (MIT)
 * Code: https://github.com/jdemeuse1204/OR-M-Data-Entities
 * Email: james.demeuse@gmail.com
 * Copyright (c) 2014 James Demeuse
 */

using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Data.SqlClient;
using System.Reflection;
using OR_M_Data_Entities.Configuration;
using OR_M_Data_Entities.Data.Definition;
using OR_M_Data_Entities.Scripts.Base;

namespace OR_M_Data_Entities.Data
{
    public abstract class Database : IDisposable
    {
        #region Properties

        protected readonly string ConnectionString;

        protected SqlConnection Connection { get; private set; }

        protected SqlCommand Command { get; set; }

        protected PeekDataReader Reader { get; set; }

        protected ConfigurationOptions Configuration { get; private set; }

        private List<KeyValuePair<Type, Type>> _tableScriptMappings { get; set; }

        public IEnumerable<KeyValuePair<Type, Type>> TableScriptMappings
        {
            get { return _tableScriptMappings; }
        }

        public void MapTableToScript<T, TK>()
            where T : class
            where TK : IReadScript<T>
        {
            _tableScriptMappings.Add(new KeyValuePair<Type, Type>(typeof(T), typeof(TK)));
        }

        private readonly IDictionary<Type, Table> _tableCache;
        #endregion

        #region Constructor
        protected Database(string connectionStringOrName)
        {
            _tableCache = new Dictionary<Type, Table>();

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

            // check to see if MARS is enabled, it is needed for transactions
            Configuration = new ConfigurationOptions(_isMARSEnabled(ConnectionString));

            Connection = new SqlConnection(ConnectionString);
        }
        #endregion

        #region Methods
        private bool _isMARSEnabled(string connectionString)
        {
            return connectionString.ToUpper().Contains("MULTIPLEACTIVERESULTSETS=TRUE");
        }

        /// <summary>
        /// Connect our SqlConnection
        /// </summary>
        protected void Connect()
        {
            const string errorMessage = "Data Context in the middle of an operation, consider locking your threads to avoid this.  Operation: {0}";

            switch (Connection.State)
            {
                case ConnectionState.Closed:
                case ConnectionState.Broken:

                    // if the connection was opened before we need to renew it
                    if (_wasConnectionPreviouslyOpened())
                    {
                        Connection.Dispose();
                        Connection = null;
                        Connection = new SqlConnection(ConnectionString);
                    }

                    Connection.Open();
                    return;
                case ConnectionState.Connecting:
                    throw new Exception(string.Format(errorMessage,"Connecting to database"));
                case ConnectionState.Executing:
                    throw new Exception(string.Format(errorMessage, "Executing Query"));
                case ConnectionState.Fetching:
                    throw new Exception(string.Format(errorMessage, "Fetching Data"));
                case ConnectionState.Open:
                    return;
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

        /// <summary>
        /// Checks to see if the connection was previously closed.  
        /// </summary>
        /// <returns></returns>
        private bool _wasConnectionPreviouslyOpened()
        {
            var innerConnection = Connection.GetType()
                .GetField("_innerConnection", BindingFlags.Instance | BindingFlags.NonPublic);

            if (innerConnection == null) throw new Exception("Cannot connect to database, inner connection not found");

            var connection = innerConnection.GetValue(Connection).GetType().Name;

            return connection.EndsWith("PreviouslyOpened");
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
        #endregion
    }
}
