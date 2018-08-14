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
        public string Sql { get; set; }
        public IEnumerable<SqlParameter> Parameters { get; set; }
    }
}
