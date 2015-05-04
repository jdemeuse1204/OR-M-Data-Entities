/*
 * OR-M Data Entities v1.1.0
 * License: The MIT License (MIT)
 * Code: https://github.com/jdemeuse1204/OR-M-Data-Entities
 * (c) 2015 James Demeuse
 */

using System.Collections.Generic;
using OR_M_Data_Entities.Infrastructure;

namespace OR_M_Data_Entities.Expressions.Resolver
{
    public sealed class ExpressionResolutionResult
    {
        public ExpressionResolutionResult(string sql, List<SqlDbParameter> parameters)
        {
            Sql = sql;
            Parameters = parameters;
        }

        public string Sql { get; private set; }

        public List<SqlDbParameter> Parameters { get; private set; }
    }
}
