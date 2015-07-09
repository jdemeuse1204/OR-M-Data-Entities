/*
 * OR-M Data Entities v2.1
 * License: The MIT License (MIT)
 * Code: https://github.com/jdemeuse1204/OR-M-Data-Entities
 * Email: james.demeuse@gmail.com
 * Copyright (c) 2015 James Demeuse
 */

using OR_M_Data_Entities.Expressions.Resolution;

namespace OR_M_Data_Entities.Data.Execution
{
    public class SqlPayload : ISqlPayload
    {
        public SqlPayload(IExpressionQueryResolvable query)
        {
            Query = query;
        }

        public IExpressionQueryResolvable Query { get; private set; }
    }
}
