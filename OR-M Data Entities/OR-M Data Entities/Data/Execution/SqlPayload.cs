/*
 * OR-M Data Entities v2.0
 * License: The MIT License (MIT)
 * Code: https://github.com/jdemeuse1204/OR-M-Data-Entities
 * Copyright (c) 2015 James Demeuse
 */
using OR_M_Data_Entities.Expressions.Resolution;

namespace OR_M_Data_Entities.Data.Execution
{
    public class SqlPayload : ISqlPayload
    {
        public SqlPayload(IExpressionQueryResolvable query, bool isLazyLoading)
        {
            Query = query;
            IsLazyLoading = isLazyLoading;
        }

        public IExpressionQueryResolvable Query { get; private set; }

        public bool IsLazyLoading { get; private set; }
    }
}
