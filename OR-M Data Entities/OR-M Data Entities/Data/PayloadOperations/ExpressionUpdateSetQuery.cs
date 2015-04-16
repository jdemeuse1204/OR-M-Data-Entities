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
    public class ExpressionUpdateSetQuery : ExpressionQuery
    {
		public ExpressionUpdateSetQuery(ObjectMap map, DataFetching context)
			: base(context)
		{
			Map = map;
		}

		public ExpressionUpdateSetQuery AddWhere<T>(Expression<Func<T, bool>> expression) where T : class
        {
            LambdaResolver.ResolveWhereExpression(expression, Map);

			return new ExpressionUpdateSetQuery(Map, Context);
        }

		public ExpressionUpdateSetQuery Set<T>(Func<T, object> expression)
		{
			// TODO fix me
			return new ExpressionUpdateSetQuery(Map, Context);
		}
    }
}
