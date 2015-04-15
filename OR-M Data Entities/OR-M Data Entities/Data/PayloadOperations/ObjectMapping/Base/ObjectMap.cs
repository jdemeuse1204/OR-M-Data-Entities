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

        public string FromTableName()
        {
            var table = Tables.First(w => w.Type == BaseType);

            return table.HasAlias ? table.Alias : table.TableName;
        }

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

            if (_tables == null)
            {
                _tables = new List<ObjectTable>();
            }

            _tables.Add(table);

            if (!HasForeignKeys) return;

            _selectRecursive(type, table);
        }

        public void AddSingleTable(Type type)
        {
            var tableName = DatabaseSchemata.GetTableName(type);
            var table = new ObjectTable(type, tableName, tableName);

            if (_tables == null)
            {
                _tables = new List<ObjectTable>();
            }

            _tables.Add(table);
        }

        public bool HasTable(string alias)
        {
            return Tables.Any(w => w.HasAlias && w.Alias.Equals(alias));
        }

        public void AddAllTables(Type type)
        {
            var tableName = DatabaseSchemata.GetTableName(type);
            HasForeignKeys = DatabaseSchemata.HasForeignKeys(type);

            var table = new ObjectTable(type, tableName, tableName);

            if (_tables == null)
            {
                _tables = new List<ObjectTable>();
            }

            _tables.Add(table);

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
                    var column = parentTable.Columns.First(w => w.IsKey);
                    var childColumn = table.Columns.First(w => w.Name == attribute.ForeignKeyColumnName);

                    column.Joins.Add(new KeyValuePair<ObjectColumn, JoinType>(childColumn, JoinType.Inner));
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
