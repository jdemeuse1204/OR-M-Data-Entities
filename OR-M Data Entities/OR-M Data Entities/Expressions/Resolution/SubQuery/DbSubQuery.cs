﻿/*
 * OR-M Data Entities v2.0
 * License: The MIT License (MIT)
 * Code: https://github.com/jdemeuse1204/OR-M-Data-Entities
 * Copyright (c) 2015 James Demeuse
 */
using OR_M_Data_Entities.Enumeration;
using OR_M_Data_Entities.Expressions.Resolution.Where;

namespace OR_M_Data_Entities.Expressions.Resolution.SubQuery
{
    public abstract class DbSubQuery<T> : DbWhereQuery<T>
    {
        protected DbSubQuery(string viewId = null)
            : base(viewId)
        {
            
        }

        protected DbSubQuery(IExpressionQueryResolvable query, ExpressionQueryConstructionType constructionType)
            : base(query, constructionType)
        {

        }
    }
}
