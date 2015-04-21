using System;
using System.Linq.Expressions;
using OR_M_Data_Entities.Data;
using OR_M_Data_Entities.Expressions.LambdaResolution;
using OR_M_Data_Entities.Expressions.ObjectMapping;
using OR_M_Data_Entities.Expressions.ObjectMapping.Base;

namespace OR_M_Data_Entities.Expressions
{
    public class ExpressionOrderedQuery : ExpressionQuery
    {
        public ExpressionOrderedQuery(ObjectMap map, DataFetching context)
			: base(context)
		{
			Map = map;
		}

        public ExpressionOrderedQuery OrderBy<T>(Expression<Func<T, object>> selector)
        {
            LambdaResolver.ResolveOrderExpression(selector, Map, ObjectColumnOrderType.Ascending);

            return new ExpressionOrderedQuery(Map, Context);
        }

        public ExpressionOrderedQuery OrderByDescending<T>(Expression<Func<T, object>> selector)
        {
            LambdaResolver.ResolveOrderExpression(selector, Map, ObjectColumnOrderType.Descending);

            return new ExpressionOrderedQuery(Map, Context);
        }
    }
}
