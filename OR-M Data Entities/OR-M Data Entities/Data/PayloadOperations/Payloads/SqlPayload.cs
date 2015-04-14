using System.Data.SqlClient;
using OR_M_Data_Entities.Data.PayloadOperations.Payloads.Base;

namespace OR_M_Data_Entities.Data.PayloadOperations.Payloads
{
    public abstract class SqlPayload : IPayload
    {
        public readonly SqlConnection _connection;

        protected SqlPayload(SqlConnection connection)
        {
            _connection = connection;
        }

        public abstract string Resolve();

        public SqlCommand ExecutePayload()
        {
            return new SqlCommand(Resolve(), _connection);
        }
    }
}
