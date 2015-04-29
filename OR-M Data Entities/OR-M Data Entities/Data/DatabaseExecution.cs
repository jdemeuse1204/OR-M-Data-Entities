using System.Collections.Generic;
using System.Data.SqlClient;
using System.Linq;
using OR_M_Data_Entities.Commands.Support;
using OR_M_Data_Entities.Data.Definition;
using OR_M_Data_Entities.Expressions;

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

        protected void ExecuteReader(SqlQuery query)
        {
            TryDisposeCloseReader();

            Command = new SqlCommand(query.ToSql(), Connection);

            _addParameters(query.Parameters);

            Connect();

            Reader = Command.ExecuteReaderWithPeeking();
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

        #endregion
    }
}
