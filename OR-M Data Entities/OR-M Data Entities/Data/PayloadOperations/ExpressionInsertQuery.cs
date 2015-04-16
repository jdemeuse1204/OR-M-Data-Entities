using OR_M_Data_Entities.Data.PayloadOperations.ObjectMapping.Base;
using OR_M_Data_Entities.Data.PayloadOperations.Payloads;
using OR_M_Data_Entities.Data.PayloadOperations.Payloads.Base;

namespace OR_M_Data_Entities.Data.PayloadOperations
{
	public class ExpressionInsertQuery : ExpressionQuery
	{
		public ExpressionInsertQuery(ObjectMap map, DataFetching context)
			: base(context)
		{
			Map = map;
		}

		public ExpressionInsertValueQuery Insert<T>() where T : class
		{
			Table<T>();

			return new ExpressionInsertValueQuery(Map, Context);
		}
	}
}
