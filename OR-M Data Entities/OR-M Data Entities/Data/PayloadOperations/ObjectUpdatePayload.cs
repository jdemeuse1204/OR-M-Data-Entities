﻿using System.Data.SqlClient;
using OR_M_Data_Entities.Data.PayloadOperations.Payloads;

namespace OR_M_Data_Entities.Data.PayloadOperations
{
    public class ObjectUpdatePayload : ObjectPayload
    {
        public ObjectUpdatePayload(SqlConnection connection)
            : base(connection)
        {
        }

        public override string Resolve()
        {
            throw new System.NotImplementedException();
        }
    }
}
