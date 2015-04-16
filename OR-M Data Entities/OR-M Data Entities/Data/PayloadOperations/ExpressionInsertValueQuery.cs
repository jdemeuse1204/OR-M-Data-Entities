using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using OR_M_Data_Entities.Data.PayloadOperations.ObjectMapping.Base;
using OR_M_Data_Entities.Data.PayloadOperations.Payloads;

namespace OR_M_Data_Entities.Data.PayloadOperations
{
    public class ExpressionInsertValueQuery : ExpressionQuery
    {
		public ExpressionInsertValueQuery(ObjectMap map, DataFetching context)
			: base(context)
		{
			Map = map;
		}

		public ExpressionInsertValueQuery Value<T>(Func<T, object> function) where T : class
		{

			return new ExpressionInsertValueQuery(Map, Context);
		}
    }
}
