/*
 * OR-M Data Entities v1.0.0
 * License: The MIT License (MIT)
 * Code: https://github.com/jdemeuse1204/OR-M-Data-Entities
 * (c) 2015 James Demeuse
 */
using System.Collections.Generic;

namespace OR_M_Data_Entities.Expressions.Resolver
{
    public sealed class ExpressionResolutionResult
    {
        public ExpressionResolutionResult(string sql, Dictionary<string, object> parameters)
        {
            Sql = sql;
            Parameters = parameters;
        }

        public string Sql { get; private set; }

        public Dictionary<string, object> Parameters { get; private set; }
    }
}
