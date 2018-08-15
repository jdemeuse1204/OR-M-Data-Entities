using OR_M_Data_Entities.Lite.Data;
using OR_M_Data_Entities.Lite.Expressions.Query;
using OR_M_Data_Entities.Lite.Extensions;
using OR_M_Data_Entities.Lite.Mapping.Schema;
using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Linq;

namespace OR_M_Data_Entities.Lite.Context
{
    internal class ExecutionContext
    {
        private readonly string connectionString;

        public ExecutionContext(string connectionString)
        {
            this.connectionString = connectionString;
        }

        private T Load<T>(SqlQuery query, Func<IPeekDataReader, T> load) where T : class, new()
        {
            using (var connection = new SqlConnection(this.connectionString))
            using (var command = new SqlCommand(query.Query, connection))
            {
                if (connection.State != System.Data.ConnectionState.Open) { connection.Open(); }

                command.Parameters.AddRange(query.Parameters.ToArray());

                using (var reader = command.ExecutePeekReader())
                {
                    return load(reader);
                }
            }
        }

        public T LoadOne<T>(SqlQuery sql, IReadOnlyDictionary<Type, TableSchema> tableSchemas) where T : class, new()
        {
            return Load(sql, (reader) =>
            {
                return default(T);
            });
        }

        public T LoadOneDefault<T>(SqlQuery sql, IReadOnlyDictionary<Type, TableSchema> tableSchemas) where T : class, new()
        {
            return Load(sql, (reader) =>
            {
                return default(T);
            });
        }

        public List<T> LoadAll<T>(SqlQuery sql, IReadOnlyDictionary<Type, TableSchema> tableSchemas) where T : class, new()
        {
            using (var connection = new SqlConnection(this.connectionString))
            using (var command = new SqlCommand(sql.Query, connection))
            {
                if (connection.State != System.Data.ConnectionState.Open) { connection.Open(); }

                command.Parameters.AddRange(sql.Parameters.ToArray());

                using (var reader = command.ExecutePeekReader())
                {
                    var result = new List<T>();
                    while (reader.Read())
                    {
                        result.Add(default(T));
                    }
                    return result;
                }
            }
        }
    }
}
