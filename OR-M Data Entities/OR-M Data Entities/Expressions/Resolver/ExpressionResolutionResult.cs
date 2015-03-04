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
