using System;
using System.Linq.Expressions;
using OR_M_Data_Entities.Commands;
using OR_M_Data_Entities.Data;
using OR_M_Data_Entities.Expressions.Operations.LambdaResolution;
using OR_M_Data_Entities.Expressions.Operations.ObjectMapping.Base;
using OR_M_Data_Entities.Expressions.Operations.Payloads;

namespace OR_M_Data_Entities.Expressions
{
    public class ExpressionSelectQuery : ExpressionQuery
    {
		public ExpressionSelectQuery(ObjectMap map, DataFetching context)
			: base(context)
		{
			Map = map;
		}

        public ExpressionWhereJoinQuery SelectAll<T>() where T : class
        {
            // rename for asethetics
            if (Map != null && Map.BaseType != null && Map.BaseType == typeof(T))
            {
                throw new Exception("Cannot return more than one data type");
            }

            Table<T>();

			return new ExpressionWhereJoinQuery(Map, Context);
        }

        public ExpressionWhereJoinQuery Include<T>() where T : class
        {
            Map.AddSingleTable(typeof(T), true);

            return new ExpressionWhereJoinQuery(Map, Context);
        }

		public ExpressionSelectQuery Take(int rows)
        {
            Map.Rows = rows;

			return new ExpressionSelectQuery(Map, Context);
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

		public ExpressionWhereQuery Where<T>(Expression<Func<T, bool>> expression) where T : class
		{
			LambdaResolver.ResolveWhereExpression(expression, Map);

			return new ExpressionWhereQuery(Map, Context);
		}
    }
}
