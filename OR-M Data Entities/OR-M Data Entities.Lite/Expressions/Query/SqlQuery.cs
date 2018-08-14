using System.Collections.Generic;
using System.Data.SqlClient;

namespace OR_M_Data_Entities.Lite.Expressions.Query
{
    internal class SqlQuery
    {
        public SqlQuery(string query, IEnumerable<SqlParameter> parameters)
        {
            Query = query;
            Parameters = parameters;
        }

        public string Query { get; }
        public IEnumerable<SqlParameter> Parameters { get; }
    }
}
