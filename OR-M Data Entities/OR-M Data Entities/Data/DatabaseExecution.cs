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

            Connect();

            Reader = Command.ExecuteReaderWithPeeking();
        }

        protected void ExecuteReader(IExpressionQueryResolvable query)
        {
            TryDisposeCloseReader();

            Command = new SqlCommand(query.Sql, Connection);

            _addParameters(query.Parameters);

            Connect();

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

            return new DataReader<T>(Reader); ;
        }

        public DataReader<T> ExecuteQuery<T>(string sql, params SqlDbParameter[] parameters)
        {
            return ExecuteQuery<T>(sql, parameters.ToList());
        }

        public DataReader<T> ExecuteQuery<T>(ISqlBuilder builder)
        {


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
