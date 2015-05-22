/*
 * OR-M Data Entities v1.2.0
 * License: The MIT License (MIT)
 * Code: https://github.com/jdemeuse1204/OR-M-Data-Entities
 * Copyright (c) 2015 James Demeuse
 */

using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Reflection;
using OR_M_Data_Entities.Commands;
using OR_M_Data_Entities.Commands.Secure.StatementParts;
using OR_M_Data_Entities.Data.Commit;
using OR_M_Data_Entities.Expressions.ObjectMapping;
using OR_M_Data_Entities.Mapping;
using OR_M_Data_Entities.Mapping.Base;

namespace OR_M_Data_Entities.Data
{
    /// <summary>
    /// This class is used to get schema information
    /// </summary>
    public static class DatabaseSchemata
    {
        public static SqlDbType GetSqlDbType(Type type)
        {
            var name = type.Name;

            if (type.IsGenericType && type.GetGenericTypeDefinition() == typeof(Nullable<>))
            {
                name = type.GetGenericArguments()[0].Name;
            }

            if (type.IsEnum) return SqlDbType.Int;

            switch (name.ToUpper())
            {
                case "INT64":
                    return SqlDbType.BigInt;
                case "INT32":
                    return SqlDbType.Int;
                case "BYTE":
                    return SqlDbType.TinyInt;
                case "BOOLEAN":
                    return SqlDbType.Bit;
                case "STRING":
                    return SqlDbType.VarChar;
                case "DATETIME":
                    return SqlDbType.DateTime;
                case "DATETIMEOFFSET":
                    return SqlDbType.DateTimeOffset;
                case "DECIMAL":
                    return SqlDbType.Decimal;
                case "SINGLE":
                    return SqlDbType.Real;
                case "INT16":
                    return SqlDbType.SmallInt;
                case "TIMESPAN":
                    return SqlDbType.Time;
                case "GUID":
                    return SqlDbType.UniqueIdentifier;
                case "XML":
                    return SqlDbType.Xml;
                case "DOUBLE":
                    return SqlDbType.Float;
                case "BYTE[]":
                    return SqlDbType.Binary;
                case "CHAR":
                    return SqlDbType.Char;
                case "OBJECT":
                    return SqlDbType.Variant;
                default:
                    throw new Exception(string.Format("Type of {0} not recognized!", name));
            }
        }

        public static bool IsPrimaryKey(MemberInfo column)
        {
            return column.Name.ToUpper() == "ID"
                || GetColumnName(column).ToUpper() == "ID"
                || column.GetCustomAttribute<KeyAttribute>() != null;
        }

        public static string GetTableName(object entity)
        {
            return GetTableName(entity.GetType());
        }

        public static string GetTableName(Type type)
        {
            // check for table name attribute
            var tableAttribute = type.GetCustomAttribute<TableAttribute>();

            return tableAttribute == null ? type.Name : tableAttribute.Name;
        }

        public static string GetTableName<T>()
        {
            return GetTableName(typeof(T));
        }

        public static string GetTableNameWithLinkedServer<T>()
        {
            return GetTableNameWithLinkedServer(typeof(T));
        }

        public static string GetTableNameWithLinkedServer(object entity)
        {
            return GetTableNameWithLinkedServer(entity.GetType());
        }

        public static string GetTableNameWithLinkedServer(Type type)
        {
            // check for table name attribute
            var tableAttribute = type.GetCustomAttribute<TableAttribute>();
            var linkedserverattribute = type.GetCustomAttribute<LinkedServerAttribute>();

            var tableName = tableAttribute == null ? type.Name : tableAttribute.Name;

            return linkedserverattribute == null
                ? tableName
                : string.Format("{0}.[{1}]", linkedserverattribute.LinkedServerText, tableName);
        }

        public static DbGenerationOption GetGenerationOption(PropertyInfo column)
        {
            var dbGenerationColumn = column.GetCustomAttribute<DbGenerationOptionAttribute>();
            return dbGenerationColumn == null ? DbGenerationOption.IdentitySpecification : dbGenerationColumn.Option;
        }

        public static string GetColumnName(MemberInfo column)
        {
            var columnAttribute = column.GetCustomAttribute<ColumnAttribute>();

            return columnAttribute == null ? column.Name : columnAttribute.Name;
        }

        public static string GetColumnName(IEnumerable<PropertyInfo> properties, string propertyName)
        {
            var property = properties.FirstOrDefault(w => w.Name == propertyName);

            // property will be in list only if it has a custom attribute
            if (property == null) return propertyName;
            var columnAttribute = property.GetCustomAttribute<ColumnAttribute>();
            return columnAttribute == null ? propertyName : columnAttribute.Name;
        }

        public static List<PropertyInfo> GetPrimaryKeys(object entity)
        {
            return GetPrimaryKeys(entity.GetType());
        }

        public static List<PropertyInfo> GetPrimaryKeys<T>()
        {
            return GetPrimaryKeys(typeof(T));
        }

        public static List<PropertyInfo> GetPrimaryKeys(Type type)
        {
            var keyList = type.GetProperties().Where(w =>
               (w.GetCustomAttributes<SearchablePrimaryKeyAttribute>() != null
               && w.GetCustomAttributes<SearchablePrimaryKeyAttribute>().Any(x => x.IsPrimaryKey))
               || (w.Name.ToUpper() == "ID")).ToList();

            if (keyList.Count != 0)
            {
                return keyList;
            }

            throw new Exception("Cannot find PrimaryKey(s)");
        }

        public static PropertyInfo GetPrimaryKeyByName(string name, Type type)
        {
            return GetPrimaryKeys(type).FirstOrDefault(w => String.Equals(w.Name, name, StringComparison.CurrentCultureIgnoreCase));
        }

        public static List<PropertyInfo> GetForeignKeys(object entity)
        {
            return GetForeignKeys(entity.GetType());
        }

        public static List<PropertyInfo> GetForeignKeys<T>()
        {
            return GetForeignKeys(typeof(T));
        }

        public static List<PropertyInfo> GetForeignKeys(Type entityType)
        {
            return entityType.GetProperties().Where(w =>
               w.GetCustomAttribute<ForeignKeyAttribute>() != null).ToList();
        }

        public static List<ForeignKeyAttribute> GetForeignKeyAttributes(Type entityType)
        {
            return entityType.GetProperties().Where(w =>
               w.GetCustomAttribute<ForeignKeyAttribute>() != null).Select(w => w.GetCustomAttribute<ForeignKeyAttribute>()).ToList();
        }

        public static List<ForeignKeyAttribute> GetForeignKeyAttributes(object entity)
        {
            return GetForeignKeyAttributes(entity.GetType());
        }

        public static List<ForeignKeyAttribute> GetForeignKeyAttributes<T>()
        {
            return GetForeignKeyAttributes(typeof(T));
        }

        public static bool HasForeignKeys<T>()
        {
            return HasForeignKeys(typeof(T));
        }

        public static bool HasForeignKeys(Type entityType)
        {
            return entityType.GetProperties().Count(w => w.GetCustomAttribute<ForeignKeyAttribute>() != null) > 0;
        }

        public static bool HasForeignKeys(object entity)
        {
            return HasForeignKeys(entity.GetType());
        }

        public static bool HasForeignListKeys(object entity)
        {
            return GetForeignKeys(entity).Any(w => w.PropertyType.IsList());
        }

        public static bool HasForeignListKeys(Type type)
        {
            return GetForeignKeys(type).Any(w => w.PropertyType.IsList());
        }

        public static bool HasForeignListKeys<T>()
        {
            return GetForeignKeys(typeof(T)).Any(w => w.PropertyType.IsList());
        }

        public static bool HasForeignNonListKeys(object entity)
        {
            return GetForeignKeys(entity).Any(w => !w.PropertyType.IsList());
        }

        public static bool HasForeignNonListKeys(Type type)
        {
            return GetForeignKeys(type).Any(w => !w.PropertyType.IsList());
        }

        public static bool HasForeignNonListKeys<T>()
        {
            return GetForeignKeys(typeof(T)).Any(w => !w.PropertyType.IsList());
        }

        public static List<PropertyInfo> GetTableFields(object entity)
        {
            return GetTableFields(entity.GetType());
        }

        public static List<PropertyInfo> GetTableFields<T>() where T : class
        {
            return GetTableFields(typeof(T));
        }

        public static List<PropertyInfo> GetTableFields(Type entityType)
        {
            return entityType.GetProperties().Where(w => w.GetCustomAttribute<UnmappedAttribute>() == null && w.GetCustomAttribute<ForeignKeyAttribute>() == null).ToList();
        }

        public static PropertyInfo GetTableFieldByName(string name, Type type)
        {
            return GetTableFields(type).FirstOrDefault(w => String.Equals(w.Name, name, StringComparison.CurrentCultureIgnoreCase));
        }

        public static KeyValuePair<string, IEnumerable<string>> GetSelectAllFieldsAndTableName(Type tableType)
        {
            var table = GetTableName(tableType);
            var fields = GetTableFields(tableType).Select(w => w.GetCustomAttribute<ColumnAttribute>() != null ? w.GetCustomAttribute<ColumnAttribute>().Name : w.Name);

            return new KeyValuePair<string, IEnumerable<string>>(table, fields);
        }

        public static KeyValuePair<string, IEnumerable<string>> GetSelectAllFieldsAndTableName<T>()
        {
            return GetSelectAllFieldsAndTableName(typeof(T));
        }

        public static List<SqlTableColumnPair> GetTableColumnPairsFromTable(Type type, string tableName)
        {
            return GetTableFields(type).Select(w => new SqlTableColumnPair
            {
                Column = w,
                Table = type,
                DataType = GetSqlDbType(w.PropertyType),
                TableNameAlias = tableName ?? GetTableName(type)
            }).ToList();
        }

        public static List<TableInfo> GetForeignKeyTypes<T>()
        {
            return GetForeignKeyTypes(typeof(T));
        }

        public static List<TableInfo> GetForeignKeyTypes(object entity)
        {
            return GetForeignKeyTypes(entity.GetType());
        }

        public static List<TableInfo> GetForeignKeyTypes(Type type)
        {
            return _createObjectList(type);
        }

        public static SqlJoinCollection GetForeignKeyJoinsRecursive<T>() where T : class
        {
            return GetForeignKeyJoinsRecursive(typeof(T));
        }

        public static SqlJoinCollection GetForeignKeyJoinsRecursive(object entity)
        {
            return GetForeignKeyJoinsRecursive(entity.GetType());
        }

        public static SqlJoinCollection GetForeignKeyJoinsRecursive(Type type)
        {
            var result = new SqlJoinCollection();

            _addForeignKeyJoinsRecursive(result, type);

            return result;
        }

        private static List<TableInfo> _createObjectList(Type type)
        {
            var typesToScan = new List<TableInfo>
            {
                new TableInfo
                {
                    TableName = GetTableName(type),
                    Type = type,
                    PrimaryKeys = _primaryKeyColumnNamesWithTableName(type, GetTableName(type)),
                }
            };

            // skip the first one because its our first object
            for (var i = 0; i < typesToScan.Count; i++)
            {
                // add the types foreign keys
                var a = typesToScan[i];

                typesToScan.AddRange(GetForeignKeys(a.Type).Select(w => new TableInfo
                {
                    ParentType = a.Type,
                    ParentProperty = a.Property,
                    Type = w.PropertyType.IsList() ? w.PropertyType.GetGenericArguments()[0] : w.PropertyType,
                    TableName = GetTableName(w.PropertyType.IsList() ? w.PropertyType.GetGenericArguments()[0] : w.PropertyType),
                    TableAlias = w.Name,
                    PrimaryKeys = _primaryKeyColumnNamesWithTableName(w.PropertyType.IsList() ? w.PropertyType.GetGenericArguments()[0] : w.PropertyType, w.Name),
                    IsList = w.PropertyType.IsList(),
                    Property = w
                }));
            }

            return typesToScan;
        }

        public static void GetForeignKeyJoinsRecursive(Type type, SqlJoinCollection joins)
        {
            _addForeignKeyJoinsRecursive(joins, type);
        }

        private static string[] _primaryKeyColumnNamesWithTableName(Type type, string tableName)
        {
            var keyList = type.GetProperties().Where(w =>
              (w.GetCustomAttributes<SearchablePrimaryKeyAttribute>() != null
              && w.GetCustomAttributes<SearchablePrimaryKeyAttribute>().Any(x => x.IsPrimaryKey))
              || (w.Name.ToUpper() == "ID")).ToList();

            var result = new string[keyList.Count];

            for (var i = 0; i < keyList.Count; i++)
            {
                var propertyInfo = keyList[i];
                var columnAttribute = propertyInfo.GetCustomAttribute<ColumnAttribute>();

                result[i] = string.Format("{0}{1}", tableName, columnAttribute == null ? propertyInfo.Name : columnAttribute.Name);
            }

            return result;
        }

        private static void _addForeignKeyJoinsRecursive(SqlJoinCollection result, Type type)
        {
            var foreignKeys = GetForeignKeys(type);

            foreach (var foreignKey in foreignKeys)
            {
                var isList = foreignKey.PropertyType.IsList();
                var foreignKeyAttribute = foreignKey.GetCustomAttribute<ForeignKeyAttribute>();
                var fkPropertyType = isList
                    ? foreignKey.PropertyType.GetGenericArguments()[0]
                    : foreignKey.PropertyType;
                var joinEntityColumn = isList ? GetTableFieldByName(foreignKeyAttribute.ForeignKeyColumnName, fkPropertyType) : GetPrimaryKeys(fkPropertyType).First();
                var parentEntityColumn = isList ? GetPrimaryKeys(type).First() : GetTableFieldByName(foreignKeyAttribute.ForeignKeyColumnName, type);

                if (joinEntityColumn == null) throw new Exception(string.Format("Could not resolve foreign key(s) for Type Type {0}", fkPropertyType.Name));

                if (parentEntityColumn == null) throw new Exception(string.Format("Could not resolve foreign key(s) for Type Type {0}", type.Name));

                result.Add(new SqlJoin
                {
                    ParentEntity = new SqlTableColumnPair
                    {
                        Table = type,
                        Column = parentEntityColumn
                    },

                    JoinEntity = new SqlTableColumnPair
                    {
                        Table = fkPropertyType,
                        Column = joinEntityColumn
                    },
                    JoinEntityTableName = foreignKey.Name,
                    Type = JoinType.Inner
                });

                if (HasForeignKeys(fkPropertyType))
                {
                    _addForeignKeyJoinsRecursive(result, fkPropertyType);
                }
            }
        }
    }
}
