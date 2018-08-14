using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OR_M_Data_Entities.Lite.Expressions
{
    internal class ResolvedWhereExpression
    {
        public ResolvedWhereExpression(string sql, IEnumerable<SqlParameter> parameters)
        {
            Sql = sql;
            Parameters = parameters;
        }

        public string Sql { get; }
        public IEnumerable<SqlParameter> Parameters { get; }
    }
}
