using System;
using System.Linq;
using System.Linq.Expressions;
using OR_M_Data_Entities.Commands;
using OR_M_Data_Entities.Data.PayloadOperations.LambdaResolution;
using OR_M_Data_Entities.Data.PayloadOperations.ObjectMapping.Base;
using OR_M_Data_Entities.Data.PayloadOperations.Payloads;
using OR_M_Data_Entities.Data.PayloadOperations.Payloads.Base;
using OR_M_Data_Entities.Data.PayloadOperations.QueryResolution;
using OR_M_Data_Entities.Data.PayloadOperations.QueryResolution.Base;

namespace OR_M_Data_Entities.Data.PayloadOperations
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

		public ExpressionSelectQuery Take(int rows)
        {
            Map.Rows = rows;

			return new ExpressionSelectQuery(Map, Context);
        }

		public ExpressionWhereQuery Where<T>(Expression<Func<T, bool>> expression) where T : class
		{
			LambdaResolver.ResolveWhereExpression(expression, Map);

			return new ExpressionWhereQuery(Map, Context);
		}
    }
}
