using System.Data.SqlClient;
using OR_M_Data_Entities.Data.PayloadOperations.Payloads.Base;
using OR_M_Data_Entities.Data.PayloadOperations.QueryResolution.Base;

namespace OR_M_Data_Entities.Data.PayloadOperations.Payloads
{
    public abstract class SqlQueryBuilder : IBuilder, IResolver
    {
        public readonly SqlConnection _connection;

        protected SqlQueryBuilder(SqlConnection connection)
        {
            _connection = connection;
        }

        public abstract string Resolve();

        public SqlCommand ExecuteBuilder()
        {
            return new SqlCommand(Resolve(), _connection);
        }
    }
}
