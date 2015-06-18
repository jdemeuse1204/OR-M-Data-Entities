/*
 * OR-M Data Entities v2.0
 * License: The MIT License (MIT)
 * Code: https://github.com/jdemeuse1204/OR-M-Data-Entities
 * Copyright (c) 2015 James Demeuse
 */

using OR_M_Data_Entities.Data;
using OR_M_Data_Entities.Enumeration;

namespace OR_M_Data_Entities.Expressions.Resolution
{
    public class ExpressionQueryViewResolvable<T> : ExpressionQueryResolvable<T>, IExpressionQueryViewResolvable
    {
        public new string ViewId {
            get { return base.ViewId; }
        }

        public ExpressionQueryViewResolvable(DatabaseReading context, string viewId)
            : base(context, viewId)
        {
        }

        public ExpressionQueryViewResolvable(IExpressionQueryResolvable query, ExpressionQueryConstructionType constructionType)
            : base(query, constructionType)
        {
        }
    }
}
