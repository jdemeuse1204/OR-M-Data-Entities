/*
 * OR-M Data Entities v1.2.0
 * License: The MIT License (MIT)
 * Code: https://github.com/jdemeuse1204/OR-M-Data-Entities
 * (c) 2015 James Demeuse
 */

using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Reflection;
using OR_M_Data_Entities.Commands;
using OR_M_Data_Entities.Commands.StatementParts;
using OR_M_Data_Entities.Mapping;
using OR_M_Data_Entities.Mapping.Base;

namespace OR_M_Data_Entities.Data
{
    public static class DatabaseSchemata
    {
        public static SqlDbType GetSqlDbType(Type type)
        {
            var name = type.Name;

            if (type.IsGenericType && type.GetGenericTypeDefinition() == typeof(Nullable<>))
            {
                name = type.GetGenericArguments()[0].Name;
            }

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
                default:
                    throw new Exception("Type not recognized!");
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

        public static List<PropertyInfo> GetPrimaryKeys(Type type)
        {
            var keyList = type.GetProperties().Where(w =>
               (w.GetCustomAttribute<SearchablePrimaryKeyAttribute>() != null
               && w.GetCustomAttribute<SearchablePrimaryKeyAttribute>().IsPrimaryKey)
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

        public static List<PropertyInfo> GetForeignKeys<T>() where T : class
        {
            return GetForeignKeys(typeof(T));
        }

        public static List<PropertyInfo> GetForeignKeys(Type entityType)
        {
            return entityType.GetProperties().Where(w =>
               w.GetCustomAttribute<ForeignKeyAttribute>() != null).ToList();
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

        public static List<SqlTableColumnPair> GetTableColumnPairsFromTable(Type type)
        {
            return GetTableFields(type).Select(w => new SqlTableColumnPair
            {
                Column = w,
                Table = type,
                DataType = GetSqlDbType(w.PropertyType)
            }).ToList();
        } 

        public static Dictionary<KeyValuePair<Type, Type>, SqlJoin> GetForeignKeyJoinsRecursive<T>(out List<Type> distinctTableTypes) where T : class
        {
            return GetForeignKeyJoinsRecursive(typeof(T), out distinctTableTypes);
        }

        public static Dictionary<KeyValuePair<Type, Type>, SqlJoin> GetForeignKeyJoinsRecursive(object entity, out List<Type> distinctTableTypes)
        {
            return GetForeignKeyJoinsRecursive(entity.GetType(), out distinctTableTypes);
        }

        public static Dictionary<KeyValuePair<Type, Type>, SqlJoin> GetForeignKeyJoinsRecursive(Type type, out List<Type> distinctTableTypes)
        {
            var result = new Dictionary<KeyValuePair<Type, Type>, SqlJoin>();
            distinctTableTypes = new List<Type>();

            _addForeignKeyJoinsRecursive(result, type, distinctTableTypes);

            return result;
        }

        private static void _addForeignKeyJoinsRecursive(Dictionary<KeyValuePair<Type, Type>, SqlJoin> result, Type type, List<Type> distinctTableTypes)
        {
            var foreignKeys = GetForeignKeys(type);

            foreach (var foreignKey in foreignKeys)
            {
                var foreignKeyAttribute = foreignKey.GetCustomAttribute<ForeignKeyAttribute>();
                var fkPropertyType = foreignKey.PropertyType.IsList()
                    ? foreignKey.PropertyType.GetGenericArguments()[0]
                    : foreignKey.PropertyType;
                var key = new KeyValuePair<Type, Type>(type, fkPropertyType);

                // make sure the join isnt already added
                if (result.ContainsKey(key)) continue;

                if (!distinctTableTypes.Contains(type))
                {
                    distinctTableTypes.Add(type);
                }

                if (!distinctTableTypes.Contains(fkPropertyType))
                {
                    distinctTableTypes.Add(fkPropertyType);
                }

                result.Add(key, new SqlJoin
                {
                    ParentEntity = new SqlTableColumnPair
                    {
                        Table = type,
                        Column = GetTableFieldByName(foreignKeyAttribute.PrimaryKeyColumnName, type)
                    },

                    JoinEntity = new SqlTableColumnPair
                    {
                        Table = fkPropertyType,
                        Column = GetTableFieldByName(foreignKeyAttribute.ForeignKeyColumnName, fkPropertyType)
                    },

                    Type = JoinType.Inner
                });

                if (HasForeignKeys(fkPropertyType))
                {
                    _addForeignKeyJoinsRecursive(result, fkPropertyType, distinctTableTypes);
                }
            }
        }
    }
}
