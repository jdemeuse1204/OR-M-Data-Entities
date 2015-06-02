﻿using OR_M_Data_Entities.Enumeration;
using OR_M_Data_Entities.Expressions.Resolution.Where;

namespace OR_M_Data_Entities.Expressions.Resolution.SubQuery
{
    public abstract class DbSubQuery<T> : DbWhereQuery<T>
    {
        protected DbSubQuery()
            : base()
        {
            
        }

        protected DbSubQuery(IExpressionQueryResolvable query, ExpressionQueryConstructionType constructionType)
            : base(query, constructionType)
        {

        }
    }
}
