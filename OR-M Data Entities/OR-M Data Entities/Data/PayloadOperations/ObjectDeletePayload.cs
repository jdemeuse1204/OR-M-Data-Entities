using System.Data.SqlClient;
using OR_M_Data_Entities.Data.PayloadOperations.Payloads;

namespace OR_M_Data_Entities.Data.PayloadOperations
{
    public class ObjectDeletePayload : ObjectPayload
    {
        public ObjectDeletePayload(SqlConnection connection)
            : base(connection)
        {
        }

        public override string Resolve()
        {
            throw new System.NotImplementedException();
        }
    }
}
