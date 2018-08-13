using OR_M_Data_Entities.Lite.Context;
using OR_M_Data_Entities.Lite.Expressions;
using OR_M_Data_Entities.Lite.Mapping.Schema;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OR_M_Data_Entities.Lite
{
    public class DbSqlLiteContext : IDisposable
    {
        private readonly string connectionString;
        private ExecutionContext ExecutionContext { get; set; }
        private static Dictionary<Type, TableSchema> ObjectSchemas { get; set; }

        public DbSqlLiteContext(string connectionString) 
        {
            this.connectionString = connectionString;

            ExecutionContext = new ExecutionContext(this.connectionString);
        }

        public IExpressionQuery<T> From<T>() where T : class
        {
            if (ObjectSchemas == null) { ObjectSchemas = new Dictionary<Type, TableSchema>(); }

            // build map of everything we need for selecting, reading, and setting data
            ObjectMapper.Map<T>(ObjectSchemas);

            return new ExpressionQuery<T>(ExecutionContext, ObjectSchemas);
        }

        public void Dispose()
        {
            ObjectSchemas = null;
        }
    }
}
