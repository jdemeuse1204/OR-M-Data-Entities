/*
 * OR-M Data Entities v2.2
 * License: The MIT License (MIT)
 * Code: https://github.com/jdemeuse1204/OR-M-Data-Entities
 * Email: james.demeuse@gmail.com
 * Copyright (c) 2015 James Demeuse
 */

using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.IO;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using OR_M_Data_Entities.Commands.Support;
using OR_M_Data_Entities.Data.Definition;
using OR_M_Data_Entities.Data.Execution;
using OR_M_Data_Entities.Expressions;
using OR_M_Data_Entities.Expressions.Resolution;
using OR_M_Data_Entities.Scripts;
using OR_M_Data_Entities.Mapping;

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

            Reader = Command.ExecuteReaderWithPeeking(new SqlPayload(query));
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

        public void ExecuteQuery(string sql, List<SqlDbParameter> parameters)
        {
            ExecuteReader(sql, parameters);

            Dispose();
        }

        public void ExecuteQuery(string sql, params SqlDbParameter[] parameters)
        {
            ExecuteQuery(sql, parameters.ToList());
        }
        #endregion

        #region Stored Procedures And Functions
        public DataReader<TReturnType> ExecuteStoredSql<TSource, TReturnType>(Expression<Func<TSource, string>> scriptSelector, params SqlDbParameter[] parameters)
            where TSource : StoredSql
        {
            var script = _returnStoredSql(scriptSelector);

            return ExecuteQuery<TReturnType>(script, parameters);
        }

        public void ExecuteStoredSql<TSource>(Expression<Func<TSource, string>> scriptSelector, params SqlDbParameter[] parameters)
            where TSource : StoredSql
        {
            var script = _returnStoredSql(scriptSelector);

            ExecuteQuery(script, parameters);
        }

        private string _returnStoredSql<TSource>(Expression<Func<TSource, string>> scriptSelector)
            where TSource : StoredSql
        {
            string script;
            var member = scriptSelector.Body as MemberExpression;

            if (member == null) throw new Exception("Usage of stored procedure incorrect.  Ex. ctx.ExecuteStoredProcedure<MyClass>(x => x.MyProcedure)");

            var property = typeof(TSource).GetProperty(member.Member.Name);
            var field = typeof(TSource).GetField(member.Member.Name);

            if (property == null)
            {
                script = field.GetValue(Activator.CreateInstance<TSource>()) as string;

                if (string.IsNullOrWhiteSpace(script))
                {
                    throw new Exception("Fields must have a return type of string and must not be null.");
                }
            }
            else
            {
                script = _getScriptText(property);
            }

            return script;
        }

        private string _getScriptText(PropertyInfo property)
        {
            var path = "../../Scripts/";
            var scriptAttribute = property.GetCustomAttribute<Script>();
            var fileName = property.Name;

            if (scriptAttribute != null)
            {
                if (!string.IsNullOrWhiteSpace(scriptAttribute.Path))
                {
                    path = string.Format("../../{0}", scriptAttribute.Path);
                }

                if (!string.IsNullOrWhiteSpace(scriptAttribute.FileName))
                {
                    fileName = scriptAttribute.FileName;
                }
            }

            var fullScriptPath = Path.Combine(path,
                string.Format("{0}{1}", fileName, fileName.ToUpper().Contains(".SQL") ? string.Empty : ".sql"));
            var result = string.Empty;

            // open the file as readonly and read line by line in case the script is large
            using (var fs = new FileStream(fullScriptPath,
                FileMode.Open,
                FileAccess.Read,
                FileShare.ReadWrite))
            {
                using (var sr = new StreamReader(fs))
                {
                    string line;

                    while ((line = sr.ReadLine()) != null)
                    {
                        result += line;
                    }
                }
            }

            return result;
        }

        #endregion
    }
}
