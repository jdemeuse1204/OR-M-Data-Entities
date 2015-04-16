using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Threading.Tasks;
using OR_M_Data_Entities.Data.PayloadOperations.LambdaResolution;
using OR_M_Data_Entities.Data.PayloadOperations.ObjectMapping.Base;
using OR_M_Data_Entities.Data.PayloadOperations.Payloads;

namespace OR_M_Data_Entities.Data.PayloadOperations
{
	public class ExpressionDeleteWhereQuery : ExpressionQuery
	{
		public ExpressionDeleteWhereQuery(ObjectMap map, DataFetching context)
			: base(context)
		{
			Map = map;
		}

		public ExpressionDeleteWhereQuery Where<T>(Expression<Func<T, bool>> expression) where T : class
		{
			LambdaResolver.ResolveWhereExpression(expression, Map);

			return new ExpressionDeleteWhereQuery(Map, Context);
		}
	}
}
