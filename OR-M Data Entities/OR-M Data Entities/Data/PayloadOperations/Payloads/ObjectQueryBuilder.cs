using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Linq;
using OR_M_Data_Entities.Data.PayloadOperations.ObjectMapping;
using OR_M_Data_Entities.Data.PayloadOperations.Payloads.Base;
using OR_M_Data_Entities.Data.PayloadOperations.QueryResolution.Base;

namespace OR_M_Data_Entities.Data.PayloadOperations.Payloads
{
    public abstract class ObjectQueryBuilder : IBuilder, IResolver
    {
        public readonly SqlConnection _connection;

        public List<ObjectTable> Tables { get; set; } // for selecting and renaming columns, should put together a map for easier object loading

        protected ObjectQueryBuilder(SqlConnection connection)
        {
            _connection = connection;
        }

        protected void Table<T>() where T : class
        {
            Tables.Add(new ObjectTable
            {
                Alias = DatabaseSchemata.GetTableName<T>(),
                Columns = DatabaseSchemata.GetTableFields<T>().Select(w => new ObjectColumn(w)).ToList(),
                TableName = DatabaseSchemata.GetTableName<T>(),
                Type = typeof(T)
            });

            if (!DatabaseSchemata.HasForeignKeys<T>()) return;

            _selectRecursive(typeof(T));
        }

        private void _selectRecursive(Type type)
        {
            foreach (var foreignKey in DatabaseSchemata.GetForeignKeys(type))
            {
                var propertyType = foreignKey.GetPropertyType();

                if (DatabaseSchemata.HasForeignKeys(propertyType))
                {
                    _selectRecursive(propertyType);
                }

                Tables.Add(new ObjectTable
                {
                    Alias = foreignKey.Name,
                    Columns = DatabaseSchemata.GetTableFields(propertyType).Select(w => new ObjectColumn(w)).ToList(),
                    TableName = DatabaseSchemata.GetTableName(propertyType),
                    Type = propertyType
                });
            }
        }

        public abstract string Resolve();

        public SqlCommand ExecuteBuilder()
        {
            return new SqlCommand(Resolve(), _connection);
        }
    }
}
