/*
 * OR-M Data Entities v3.0
 * License: The MIT License (MIT)
 * Code: https://github.com/jdemeuse1204/OR-M-Data-Entities
 * Email: james.demeuse@gmail.com
 * Copyright (c) 2014 James Demeuse
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
