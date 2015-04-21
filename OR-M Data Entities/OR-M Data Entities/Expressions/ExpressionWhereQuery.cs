﻿/*
 * OR-M Data Entities v1.2.0
 * License: The MIT License (MIT)
 * Code: https://github.com/jdemeuse1204/OR-M-Data-Entities
 * Copyright (c) 2015 James Demeuse
 */
using System;
using System.Linq.Expressions;
using OR_M_Data_Entities.Data;
using OR_M_Data_Entities.Expressions.LambdaResolution;
using OR_M_Data_Entities.Expressions.ObjectMapping;
using OR_M_Data_Entities.Expressions.ObjectMapping.Base;

namespace OR_M_Data_Entities.Expressions
{
	public class ExpressionWhereQuery : ExpressionQuery
	{
		public ExpressionWhereQuery(ObjectMap map, DataFetching context)
			: base(context)
		{
			Map = map;
		}

		public ExpressionWhereQuery Where<T>(Expression<Func<T, bool>> expression) where T : class
		{
			LambdaResolver.ResolveWhereExpression(expression, Map);

			return new ExpressionWhereQuery(Map, Context);
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
