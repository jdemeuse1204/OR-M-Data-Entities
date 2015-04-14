using System.Data.SqlClient;
using OR_M_Data_Entities.Data.PayloadOperations.Payloads.Base;

namespace OR_M_Data_Entities.Data.PayloadOperations.Payloads
{
    public abstract class ObjectPayload : IPayload
    {
        public readonly SqlConnection _connection;

        protected ObjectPayload(SqlConnection connection)
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
