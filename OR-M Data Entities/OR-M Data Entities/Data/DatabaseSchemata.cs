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
using OR_M_Data_Entities.Expressions.Support;
using OR_M_Data_Entities.Mapping;
using OR_M_Data_Entities.Mapping.Base;

namespace OR_M_Data_Entities.Data
{
    public static class DatabaseSchemata
    {
        public static bool UseTableColumnFetch(SqlExpressionType expressionType)
        {
            switch (expressionType)
            {
                case SqlExpressionType.ForeignKeySelect:
                case SqlExpressionType.ForeignKeySelectJoin:
                case SqlExpressionType.ForeignKeySelectWhere:
                case SqlExpressionType.ForeignKeySelectWhereJoin:
                    return true;
                default:
                    return false;
            }
        }

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
                case "BYTE[]":
                    return SqlDbType.Binary;
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

        public static IEnumerable<ForeignKeyDetail> GetForeignKeyTypes<T>()
        {
            return GetForeignKeyTypes(typeof (T));
        }

        public static IEnumerable<ForeignKeyDetail> GetForeignKeyTypes(object entity)
        {
            return GetForeignKeyTypes(entity.GetType());
        }

        public static IEnumerable<ForeignKeyDetail> GetForeignKeyTypes(Type type)
        {
            var result = new List<ForeignKeyDetail>();
            var resultingTypes = new List<PulledForeignKeyDetail>();

            _addForeignKeyTypesRecursive(result, type, resultingTypes);
            
            return result;
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

        public static void GetForeignKeyJoinsRecursive(Type type, SqlJoinCollection joins)
        {
            _addForeignKeyJoinsRecursive(joins, type);
        }

        private static string[] _primaryKeyColumnNamesWithTableName(Type type)
        {
            var keyList = type.GetProperties().Where(w =>
              (w.GetCustomAttributes<SearchablePrimaryKeyAttribute>() != null
              && w.GetCustomAttributes<SearchablePrimaryKeyAttribute>().Any(x => x.IsPrimaryKey))
              || (w.Name.ToUpper() == "ID")).ToList();

            var tableName = GetTableName(type);
            var result = new string[keyList.Count];

            for (var i = 0; i < keyList.Count; i++)
            {
                var propertyInfo = keyList[i];
                var columnAttribute = propertyInfo.GetCustomAttribute<ColumnAttribute>();

                result[i] = string.Format("{0}{1}", tableName, columnAttribute == null ? propertyInfo.Name : columnAttribute.Name);
            }

            return result;
        }

        private static void _addForeignKeyTypesRecursive(List<ForeignKeyDetail> result, Type type, List<PulledForeignKeyDetail> resultingTypes)
        {
            var foreignKeys = GetForeignKeys(type);

            foreach (var foreignKey in foreignKeys)
            {
                var pulledForeignKeyDetail = new PulledForeignKeyDetail(foreignKey);

                if (resultingTypes.Contains(pulledForeignKeyDetail))
                {
                    continue;
                }

                var isList = foreignKey.PropertyType.IsList();
                var fkPropertyType = isList
                    ? foreignKey.PropertyType.GetGenericArguments()[0]
                    : foreignKey.PropertyType;
                
                if (!result.Select(w => w.Type).Contains(fkPropertyType))
                {
                    var fkDetail = new ForeignKeyDetail
                    {
                        Type = fkPropertyType,
                        ParentType = type,
                        IsList = isList,
                        ListType = isList ? foreignKey.PropertyType : null,
                        PropertyName = foreignKey.Name,
                        ChildTypes = new List<ForeignKeyDetail>(),
                        PrimaryKeyDatabaseNames = _primaryKeyColumnNamesWithTableName(fkPropertyType),
                        KeysSelectedHashCodeList = new Dictionary<int, List<int>>()
                    };

                    _getChildren(fkDetail.ChildTypes, fkPropertyType, resultingTypes);

                    resultingTypes.Add(pulledForeignKeyDetail);

                    result.Add(fkDetail);
                }

                if (HasForeignKeys(fkPropertyType))
                {
                    _addForeignKeyTypesRecursive(result, fkPropertyType, resultingTypes);
                }
            }
        }

        private static void _getChildren(List<ForeignKeyDetail> result, Type type, List<PulledForeignKeyDetail> resultingTypes)
        {
            var foreignKeys = GetForeignKeys(type);

            foreach (var foreignKey in foreignKeys)
            {
                var isList = foreignKey.PropertyType.IsList();
                var fkPropertyType = isList
                    ? foreignKey.PropertyType.GetGenericArguments()[0]
                    : foreignKey.PropertyType;
                var fkDetail = new ForeignKeyDetail
                {
                    Type = fkPropertyType,
                    ParentType = type,
                    IsList = isList,
                    ListType = isList ? foreignKey.PropertyType : null,
                    PropertyName = foreignKey.Name,
                    ChildTypes = new List<ForeignKeyDetail>(),
                    PrimaryKeyDatabaseNames = _primaryKeyColumnNamesWithTableName(fkPropertyType),
                    KeysSelectedHashCodeList = new Dictionary<int, List<int>>()
                };

                result.Add(fkDetail);

                resultingTypes.Add(new PulledForeignKeyDetail(foreignKey));

                if (HasForeignKeys(fkPropertyType))
                {
                    _getChildren(fkDetail.ChildTypes, fkPropertyType, resultingTypes);
                }
            }
        }

        private static void _addForeignKeyJoinsRecursive(SqlJoinCollection result, Type type)
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
                var hasFKs = HasForeignKeys(fkPropertyType);
                var matchResult = result.ContainsKey(key);

                if (matchResult == SqlJoinCollectionKeyMatchType.AsIs ||
                    matchResult == SqlJoinCollectionKeyMatchType.Inverse)
                {
                    if (foreignKeyAttribute.IsNullable)
                    {
                        result.ChangeJoinType(key,JoinType.Left);
                    }

                    if (!hasFKs)
                    {
                        continue;
                    }

                    _addForeignKeyJoinsRecursive(result, fkPropertyType);
                    continue;
                }

                var joinEntityColumn = GetTableFieldByName(foreignKeyAttribute.ForeignKeyColumnName, fkPropertyType);
                var parentEntityColumn = GetTableFieldByName(foreignKeyAttribute.PrimaryKeyColumnName, type);

                if (joinEntityColumn == null) throw new Exception(string.Format("Cannot Find Column {0} from Type {1}", foreignKeyAttribute.ForeignKeyColumnName, fkPropertyType.Name));

                if (parentEntityColumn == null) throw new Exception(string.Format("Cannot Find Column {0} from Type {1}", foreignKeyAttribute.PrimaryKeyColumnName, type.Name));

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

                    Type = foreignKeyAttribute.IsNullable ? JoinType.Left : JoinType.Inner
                });

                if (HasForeignKeys(fkPropertyType))
                {
                    _addForeignKeyJoinsRecursive(result, fkPropertyType);
                }
            }
        }
    }
}
