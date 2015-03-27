using System.Collections.Generic;

namespace OR_M_Data_Entities.Expressions.Types.Base
{
    public class SqlExpressionResult
    {
        public string Sql { get; set; }

        public IDictionary<string, object> Parameters { get; set; }
    }
}
