using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using OR_M_Data_Entities.Commands;
using OR_M_Data_Entities.Mapping;

namespace OR_M_Data_Entities.Data.PayloadOperations.ObjectMapping.Base
{
    public class ObjectMap
    {
        public Type BaseType { get; private set; }
        public bool HasForeignKeys { get; private set; }
        public int? Rows { get; set; }

        public IEnumerable<ObjectTable> Tables
        {
            get { return _tables; }
        }
        private List<ObjectTable> _tables { get; set; }

        public ObjectMap(Type type)
        {
            var tableName = DatabaseSchemata.GetTableName(type);
            HasForeignKeys = DatabaseSchemata.HasForeignKeys(type);
            BaseType = type;
            var table = new ObjectTable(type, tableName, tableName);
            _tables = new List<ObjectTable>
            {
                table
            };

            if (!HasForeignKeys) return;

            _selectRecursive(type, table);
        }

        private void _selectRecursive(Type type, ObjectTable parentTable)
        {
            foreach (var foreignKey in DatabaseSchemata.GetForeignKeys(type))
            {
                var propertyType = foreignKey.GetPropertyType();
                var table = new ObjectTable(propertyType, foreignKey.Name, DatabaseSchemata.GetTableName(propertyType));
                var attribute = foreignKey.GetCustomAttribute<ForeignKeyAttribute>();

                if (foreignKey.PropertyType.IsList())
                {
                    var childColumn = table.Columns.First(w => w.Name == attribute.ForeignKeyColumnName);
                    var column = parentTable.Columns.First(w => w.IsKey);

                    childColumn.Joins.Add(new KeyValuePair<ObjectColumn, JoinType>(column, JoinType.Inner));
                }
                else
                {
                    var column = parentTable.Columns.First(w => w.Name == attribute.ForeignKeyColumnName);
                    var childColumn = table.Columns.First(w => w.IsKey);

                    column.Joins.Add(new KeyValuePair<ObjectColumn, JoinType>(childColumn, JoinType.Inner)); 
                }

                _tables.Add(table);

                if (DatabaseSchemata.HasForeignKeys(propertyType))
                {
                    _selectRecursive(propertyType, table);
                }
            }
        }
    }
}
