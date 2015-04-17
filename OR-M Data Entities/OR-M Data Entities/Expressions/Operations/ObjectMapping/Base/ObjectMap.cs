using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using OR_M_Data_Entities.Commands;
using OR_M_Data_Entities.Data;
using OR_M_Data_Entities.Expressions.Operations.Payloads.Base;
using OR_M_Data_Entities.Mapping;

namespace OR_M_Data_Entities.Expressions.Operations.ObjectMapping.Base
{
    public class ObjectMap : Builder
    {
        public Type BaseType { get; private set; }
        public ObjectMapReturnType DataReturnType { get; set; }
        public int? Rows { get; set; }
        public bool IsDistinct { get; set; }
        public int MemberInitCount { get; set; } // if returning a concrete type and not a dynamic this must be set to 1

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
            var hasForeignKeys = DatabaseSchemata.HasForeignKeys(type);
            DataReturnType = hasForeignKeys ? ObjectMapReturnType.ForeignKeys : ObjectMapReturnType.Basic;
            BaseType = type;
            var table = new ObjectTable(type, tableName, tableName, true, true);

            if (_tables == null)
            {
                _tables = new List<ObjectTable>();
            }

            _tables.Add(table);

            if (!hasForeignKeys) return;

            _selectRecursive(type, table);
        }

        public void AddSingleTable(Type type, bool includeInResult = false)
        {
            var tableName = DatabaseSchemata.GetTableName(type);
            var table = new ObjectTable(type, tableName, tableName, false, includeInResult);

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
            var hasForeignKeys = DatabaseSchemata.HasForeignKeys(type);
            DataReturnType = hasForeignKeys ? ObjectMapReturnType.ForeignKeys : ObjectMapReturnType.Basic;

            var table = new ObjectTable(type, tableName, tableName, false, hasForeignKeys);

            if (_tables == null)
            {
                _tables = new List<ObjectTable>();
            }

            _tables.Add(table);

            if (!hasForeignKeys) return;

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

                    // can be one or none to many
                    column.Joins.Add(new KeyValuePair<ObjectColumn, JoinType>(childColumn, JoinType.Left));
                }
                else
                {
                    var column = parentTable.Columns.First(w => w.Name == attribute.ForeignKeyColumnName);
                    var childColumn = table.Columns.First(w => w.IsKey);

                    // must exist
                    column.Joins.Add(new KeyValuePair<ObjectColumn, JoinType>(childColumn, JoinType.Inner)); 
                }

                _tables.Add(table);

                if (DatabaseSchemata.HasForeignKeys(propertyType))
                {
                    _selectRecursive(propertyType, table);
                }
            }
        }

        protected override BuildContainer Build()
        {
            throw new NotImplementedException();
        }
    }
}
