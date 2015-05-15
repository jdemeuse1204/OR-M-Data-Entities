﻿using OR_M_Data_Entities.Expressions.Query;
using OR_M_Data_Entities.Expressions.Resolution.Base;

namespace OR_M_Data_Entities.Expressions.Resolution
{
    public class ExpressionQueryFunctionResolver : ExpressionResolver
    {
        public ExpressionQueryFunctionResolver(DbQuery query)
            : base(query)
        {
        }

        public void ResolveDistinct()
        {
            SelectList.IsSelectDistinct = true;
        }

        public void ResolveTake(int rows)
        {
            SelectList.TakeRows = rows;
        }
    }
}
