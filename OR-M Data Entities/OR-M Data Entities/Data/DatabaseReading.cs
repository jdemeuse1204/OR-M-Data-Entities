/*
 * OR-M Data Entities v1.2.0
 * License: The MIT License (MIT)
 * Code: https://github.com/jdemeuse1204/OR-M-Data-Entities
 * Copyright (c) 2015 James Demeuse
 */

using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Linq.Expressions;
using OR_M_Data_Entities.Commands.Support;
using OR_M_Data_Entities.Expressions;

namespace OR_M_Data_Entities.Data
{
    /// <summary>
    /// All data reading methods in this class require a READ before data can be retreived
    /// </summary>
    public abstract class DatabaseReading : Database
    {
        #region Constructor
        protected DatabaseReading(string connectionStringOrName)
            : base(connectionStringOrName)
        {
        }
        #endregion

        #region Reader Methods
        /// <summary>
        /// Used for looping through results
        /// </summary>
        /// <returns></returns>
		protected bool Read()
        {
            if (Reader.Read())
            {
                return true;
            }

            // close reader when no rows left
            Reader.Close();
            Reader.Dispose();
            return false;
        }

        /// <summary>
        /// Converts an object to a dynamic
        /// </summary>
        /// <returns></returns>
		protected dynamic Select()
        {
            return Reader.ToObject();
        }

        /// <summary>
        /// Converts a datareader to an object of type T
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <returns></returns>
		protected T Select<T>()
        {
            return Reader.ToObject<T>();
        }
        #endregion

        #region Data Execution
        /// <summary>
        /// Execute the SqlBuilder on the database
        /// </summary>
        /// <param name="builder"></param>
		protected void Execute(ISqlBuilder builder)
        {
            _tryCloseReader();
            DataQueryType queryType;

            Command = builder.Build(Connection, out queryType);
            Connect();
            Reader = Command.ExecuteReaderWithPeeking(null);
        }

        /// <summary>
        /// Execute sql statement without sql builder on the database, this should be used for any stored
        /// procedures.  NOTE:  This does not use SqlSecureExecutable to ensure only safe sql strings
        /// are executed
        /// </summary>
        /// <param name="sql"></param>
		protected void Execute(string sql)
        {
            _tryCloseReader();

            Command = new SqlCommand(sql, Connection);

            Connect();
            Reader = Command.ExecuteReaderWithPeeking(null);
        }

        protected void Execute(string sql, Dictionary<string, object> parameters, DataQueryType dataQueryType)
        {
            _tryCloseReader();

            Command = new SqlCommand(sql, Connection);

            foreach (var item in parameters)
            {
                Command.Parameters.Add(Command.CreateParameter()).ParameterName = item.Key;
                Command.Parameters[item.Key].Value = item.Value;
            }

            Connect();
            Reader = Command.ExecuteReaderWithPeeking(null);
        }

        protected ExpressionQuery Execute<T>(Expression<Func<T, bool>> propertyLambda, DataFetching fetching)
            where T : class
        {
            return fetching.From<T>().Where(propertyLambda);
        }
        #endregion

        #region Query Execution
        public DataReader<T> ExecuteQuery<T>(string sql)
		{
			Execute(sql);

            return new DataReader<T>(Reader);
		}

        public DataReader<T> ExecuteQuery<T>(string sql, Dictionary<string, object> parameters, DataQueryType dataQueryType)
        {
            Execute(sql, parameters, dataQueryType);

            return new DataReader<T>(Reader);
        }

        public DataReader<T> ExecuteQuery<T>(ISqlBuilder builder)
        {
            Execute(builder);

            return new DataReader<T>(Reader);
        }

        private void _tryCloseReader()
        {
            if (Reader == null) return;
            Reader.Close();
            Reader.Dispose();
        }

        #endregion
    }
}
