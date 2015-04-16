using System;
using System.Data.SqlClient;
using System.Linq.Expressions;
using OR_M_Data_Entities.Data.PayloadOperations.LambdaResolution;
using OR_M_Data_Entities.Data.PayloadOperations.ObjectMapping.Base;
using OR_M_Data_Entities.Data.PayloadOperations.Payloads;
using OR_M_Data_Entities.Data.PayloadOperations.Payloads.Base;

namespace OR_M_Data_Entities.Data.PayloadOperations
{
    public class ExpressionUpdateQuery : ExpressionQuery
    {
		public ExpressionUpdateQuery(ObjectMap map, DataFetching context)
			: base(context)
		{
			Map = map;
		}

        public ExpressionUpdateSetQuery Update<T>() where T : class
        {
            Table<T>();

			return new ExpressionUpdateSetQuery(Map, Context);
        }
    }
}
