using System.Collections.Generic;
using System.Data.SqlClient;
using System.Linq;
using OR_M_Data_Entities.Data.PayloadOperations.ObjectMapping;
using OR_M_Data_Entities.Data.PayloadOperations.Payloads;

namespace OR_M_Data_Entities.Data.PayloadOperations
{
    public class ObjectSelectPayload : ObjectPayload
    {
        public List<ObjectTable> ObjectDetails { get; set; } 

        public ObjectSelectPayload(SqlConnection connection) : base(connection)
        {
            ObjectDetails = new List<ObjectTable>();
        }

        public void Select<T>()
        {
            ObjectDetails.Add(new ObjectTable
            {
                Alias = DatabaseSchemata.GetTableName<T>(),
                Columns = typeof(T).GetProperties().ToList(),
                TableName = DatabaseSchemata.GetTableName<T>(),
                Type = typeof(T)
            });
        }

        public override string Resolve()
        {
            throw new System.NotImplementedException();
        }
    }
}
