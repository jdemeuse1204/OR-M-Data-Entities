/*
 * OR-M Data Entities v1.2.0
 * License: The MIT License (MIT)
 * Code: https://github.com/jdemeuse1204/OR-M-Data-Entities
 * Copyright (c) 2015 James Demeuse
 */

using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using OR_M_Data_Entities.Commands;
using OR_M_Data_Entities.Data;
using OR_M_Data_Entities.Mapping;

namespace OR_M_Data_Entities.Expressions.ObjectMapping.Base
{
    public class ObjectMap
    {
        public Type BaseType { get; private set; }
        public ObjectMapReturnType DataReturnType { get; set; }
        public int? Rows { get; set; }
        public bool IsDistinct { get; set; }
        public int MemberInitCount { get; set; } // if returning a concrete type and not a dynamic this must be set to 1
        public int OrderSequenceCount { get; set; }

        public string FromTableName()
        {
            var table = Tables.First(w => w.Type == BaseType);

            var fromTableName = table.HasAlias ? table.Alias : table.TableName;

            if (table.LinkedServer != null)
            {
                return string.Format("{0}.[{1}] AS [{2}] ", table.LinkedServer.LinkedServerText, table.TableName,
                    fromTableName);
            }

            return string.Format("[{0}] ", fromTableName);
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

            var indexCount = OrderSequenceCount;

            foreach (var item in Tables)
            {
                item.OrderByPrimaryKeys(ref indexCount);
            }

            OrderSequenceCount = indexCount;
        }

        public void AddSingleTable(Type type, bool includeInResult = false)
        {
            var tableName = DatabaseSchemata.GetTableName(type);
            var table = new ObjectTable(type, tableName, tableName, false, includeInResult);

            if (_tables == null)
            {
                _tables = new List<ObjectTable>();
            }


            var indexCount = OrderSequenceCount;

            if (DataReturnType == ObjectMapReturnType.ForeignKeys)
            {
                table.OrderByPrimaryKeys(ref indexCount);
            }

            OrderSequenceCount = indexCount;

            _tables.Add(table);
        }

        public IOrderedEnumerable<ObjectColumn> OrderByColumns()
        {
            var result = new List<ObjectColumn>();

            foreach (var table in Tables.Where(w => w.HasOrderSequence()))
            {
                result.AddRange(table.GetOrderByColumns());
            }

            return result.OrderBy(w => w.SequenceNumber);
        }

        public bool HasTable(string alias)
        {
            return Tables.Any(w => w.HasAlias && w.Alias.Equals(alias));
        }

        public bool HasOrderSequence()
        {
            return Tables.Any(w => w.HasOrderSequence());
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

        private void _selectRecursive(Type type, ObjectTable parentTable, bool isComingFromList = false)
        {
            foreach (var foreignKey in DatabaseSchemata.GetForeignKeys(type))
            {
                var propertyType = foreignKey.GetPropertyType();
                var table = new ObjectTable(propertyType, foreignKey.Name, DatabaseSchemata.GetTableName(propertyType));
                var attribute = foreignKey.GetCustomAttribute<ForeignKeyAttribute>();
                var isList = foreignKey.PropertyType.IsList();

                // if a left join occurs all subsequent joins should be left joined
                if (!isComingFromList && isList)
                {
                    isComingFromList = true;
                }

                if (isList)
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
                    column.Joins.Add(new KeyValuePair<ObjectColumn, JoinType>(childColumn, isComingFromList ? JoinType.Left : JoinType.Inner)); 
                }

                _tables.Add(table);

                if (DatabaseSchemata.HasForeignKeys(propertyType))
                {
                    _selectRecursive(propertyType, table, isComingFromList);
                }
            }
        }
    }
}
