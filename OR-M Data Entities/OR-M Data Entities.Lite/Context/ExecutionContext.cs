using OR_M_Data_Entities.Lite.Data;
using OR_M_Data_Entities.Lite.Extensions;
using OR_M_Data_Entities.Lite.Mapping.Schema;
using System;
using System.Collections.Generic;
using System.Data.SqlClient;

namespace OR_M_Data_Entities.Lite.Context
{
    internal class ExecutionContext
    {
        private readonly string connectionString;

        public ExecutionContext(string connectionString)
        {
            this.connectionString = connectionString;
        }

        private T Load<T>(string sql, Func<IPeekDataReader, T> load)
        {
            using (var connection = new SqlConnection(this.connectionString))
            using (var command = new SqlCommand(sql, connection))
            {
                if (connection.State != System.Data.ConnectionState.Open) { connection.Open(); }

                using (var reader = command.ExecutePeekReader())
                {
                    return load(reader);
                }
            }
        }

        public T LoadOne<T>(string sql, IReadOnlyDictionary<Type, TableSchema> tableSchemas)
        {
            return Load(sql, (reader) =>
            {
                return reader.ToObject<T>();
            });
        }

        public T LoadOneDefault<T>(string sql, IReadOnlyDictionary<Type, TableSchema> tableSchemas)
        {
            return Load(sql, (reader) =>
            {
                return reader.ToObjectDefault<T>();
            });
        }

        public List<T> LoadAll<T>(string sql, IReadOnlyDictionary<Type, TableSchema> tableSchemas)
        {
            using (var connection = new SqlConnection(this.connectionString))
            using (var command = new SqlCommand(sql, connection))
            {
                if (connection.State != System.Data.ConnectionState.Open) { connection.Open(); }

                using (var reader = command.ExecutePeekReader())
                {
                    var result = new List<T>();
                    while (reader.Read())
                    {
                        result.Add(reader.ToObject<T>());
                    }
                    return result;
                }
            }
        }
    }
}
