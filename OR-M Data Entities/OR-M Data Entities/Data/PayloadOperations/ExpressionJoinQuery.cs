using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Threading.Tasks;
using OR_M_Data_Entities.Commands;
using OR_M_Data_Entities.Data.PayloadOperations.LambdaResolution;
using OR_M_Data_Entities.Data.PayloadOperations.ObjectMapping.Base;
using OR_M_Data_Entities.Data.PayloadOperations.Payloads;

namespace OR_M_Data_Entities.Data.PayloadOperations
{
	public class ExpressionJoinQuery : ExpressionQuery
	{
		public ExpressionJoinQuery(ObjectMap map, DataFetching context)
			: base(context)
		{
			Map = map;
		}

		public ExpressionWhereQuery Where<T>(Expression<Func<T, bool>> expression) where T : class
		{
			LambdaResolver.ResolveWhereExpression(expression, Map);

			return new ExpressionWhereQuery(Map, Context);
		}

		public ExpressionJoinQuery InnerJoin<TParent, TChild>(Expression<Func<TParent, TChild, bool>> expression)
			where TParent : class
			where TChild : class
		{
			LambdaResolver.ResolveJoinExpression(expression, JoinType.Inner, Map);

			return new ExpressionJoinQuery(Map, Context);
		}

		public ExpressionJoinQuery LeftJoin<TParent, TChild>(Expression<Func<TParent, TChild, bool>> expression)
			where TParent : class
			where TChild : class
		{
			LambdaResolver.ResolveJoinExpression(expression, JoinType.Left, Map);

			return new ExpressionJoinQuery(Map, Context);
		}
	}
}
