/*
 * OR-M Data Entities v2.0
 * License: The MIT License (MIT)
 * Code: https://github.com/jdemeuse1204/OR-M-Data-Entities
 * Copyright (c) 2015 James Demeuse
 */

using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Linq;
using OR_M_Data_Entities.Commands.Support;
using OR_M_Data_Entities.Data.Definition;
using OR_M_Data_Entities.Data.Execution;
using OR_M_Data_Entities.Expressions;
using OR_M_Data_Entities.Expressions.Resolution;

namespace OR_M_Data_Entities.Data
{
    public abstract class DatabaseExecution : Database
    {
        protected DatabaseExecution(string connectionStringOrName)
            : base(connectionStringOrName)
        {
        }

        private void _addParameters(IEnumerable<SqlDbParameter> parameters)
        {
            foreach (var item in parameters)
            {
                Command.Parameters.Add(Command.CreateParameter()).ParameterName = item.Name;
                Command.Parameters[item.Name].Value = item.Value;
            }
        }

        protected void ExecuteReader(string sql)
        {
            ExecuteReader(sql, new List<SqlDbParameter>());
        }

        protected void ExecuteReader(string sql, List<SqlDbParameter> parameters)
        {
            TryDisposeCloseReader();

            Command = new SqlCommand(sql, Connection);

            _addParameters(parameters);

            if (!Connect())
            {
                Command = new SqlCommand(sql, Connection);

                _addParameters(parameters);

                if (!Connect()) throw new Exception("Cannot connect to server");
            }

            Reader = Command.ExecuteReaderWithPeeking();
        }

        /// <summary>
        /// Execute the SqlBuilder on the database
        /// </summary>
        /// <param name="builder"></param>
        protected void ExecuteReader(ISqlBuilder builder)
        {
            TryDisposeCloseReader();

            Command = builder.Build(Connection);

            if (!Connect())
            {
                Command = builder.Build(Connection);

                if (!Connect()) throw new Exception("Cannot connect to server");
            }

            Reader = Command.ExecuteReaderWithPeeking();
        }

        protected void ExecuteReader(IExpressionQueryResolvable query)
        {
            TryDisposeCloseReader();

            Command = new SqlCommand(query.Sql, Connection);

            _addParameters(query.Parameters);

            if (!Connect())
            {
                Command = new SqlCommand(query.Sql, Connection);

                _addParameters(query.Parameters);

                if (!Connect()) throw new Exception("Cannot connect to server");
            }

            Reader = Command.ExecuteReaderWithPeeking(new SqlPayload(query, IsLazyLoadEnabled));
        }

        #region Query Execution
        public DataReader<T> ExecuteQuery<T>(string sql)
        {
            ExecuteReader(sql);

            return new DataReader<T>(Reader);
        }

        public DataReader<T> ExecuteQuery<T>(string sql, List<SqlDbParameter> parameters)
        {
            ExecuteReader(sql, parameters);

            return new DataReader<T>(Reader);
        }

        public DataReader<T> ExecuteQuery<T>(string sql, params SqlDbParameter[] parameters)
        {
            return ExecuteQuery<T>(sql, parameters.ToList());
        }

        public DataReader<T> ExecuteQuery<T>(ISqlBuilder builder)
        {
            ExecuteReader(builder);

            return new DataReader<T>(Reader);
        }


        public DataReader<T> ExecuteQuery<T>(ExpressionQuery<T> query)
        {
            var resolvableQuery = (IExpressionQueryResolvable)query;

            // execute query
            resolvableQuery.ResolveExpression();

            ExecuteReader(resolvableQuery);

            return new DataReader<T>(Reader);
        }
        #endregion
    }
}
