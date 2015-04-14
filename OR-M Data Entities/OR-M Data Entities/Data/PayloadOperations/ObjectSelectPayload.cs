using System.Collections.Generic;
using System.Data.SqlClient;
using OR_M_Data_Entities.Data.PayloadOperations.ObjectMapping;
using OR_M_Data_Entities.Data.PayloadOperations.Payloads;

namespace OR_M_Data_Entities.Data.PayloadOperations
{
    public class ObjectSelectPayload : ObjectPayload
    {
        public List<ObjectDetail> ObjectDetails { get; set; } 

        public ObjectSelectPayload(SqlConnection connection) : base(connection)
        {
            ObjectDetails = new List<ObjectDetail>();
        }

        public void Select<T>()

        public override string Resolve()
        {
            throw new System.NotImplementedException();
        }
    }
}
