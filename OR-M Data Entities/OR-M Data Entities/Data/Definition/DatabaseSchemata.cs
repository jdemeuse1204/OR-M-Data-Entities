/*
 * OR-M Data Entities v2.0
 * License: The MIT License (MIT)
 * Code: https://github.com/jdemeuse1204/OR-M-Data-Entities
 * Copyright (c) 2015 James Demeuse
 */

using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Reflection;
using OR_M_Data_Entities.Data.Definition.Base;
using OR_M_Data_Entities.Enumeration;
using OR_M_Data_Entities.Expressions.Query;
using OR_M_Data_Entities.Expressions.Query.Columns;
using OR_M_Data_Entities.Expressions.Resolution.Join;
using OR_M_Data_Entities.Mapping;
using OR_M_Data_Entities.Mapping.Base;

namespace OR_M_Data_Entities.Data.Definition
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

        public static ObjectSchematic GetObjectSchematic<T>(string viewId, bool lasyLoad = false)
        {
            return GetObjectSchematic(typeof(T), viewId);
        }

        public static ObjectSchematic GetObjectSchematic(object entity, string viewId, bool lasyLoad = false)
        {
            return GetObjectSchematic(entity.GetType(), viewId);
        }

        public static ObjectSchematic GetObjectSchematic(Type type, string viewId, bool lasyLoad = false)
        {
            var foreignKeys = new List<ObjectSchematic>();
            var resultingTypes = new List<PulledForeignKeyDetail>();

            _addForeignKeyTypesRecursive(foreignKeys, type, resultingTypes, viewId, lasyLoad);

            var isList = type.IsList();

            var result = new ObjectSchematic
            {
                Type = type,
                ParentType = type,
                IsList = isList,
                ListType = isList ? type.GetGenericArguments()[0] : null,
                PropertyName = type.Name,
                TableName = GetTableName(type),
                ChildTypes = foreignKeys,
                PrimaryKeyDatabaseNames = _primaryKeyColumnNamesWithTableName(type),
            };

            _loadColumnSchematics(result, type, type.Name);

            return result;
        }

        private static void _addForeignKeyTypesRecursive(List<ObjectSchematic> result, Type type, List<PulledForeignKeyDetail> resultingTypes, string viewId, bool lasyLoad)
        {
            var foreignKeys = GetForeignKeys(type, viewId);

            foreach (var foreignKey in foreignKeys)
            {
                var pulledForeignKeyDetail = new PulledForeignKeyDetail(foreignKey);

                if (resultingTypes.Contains(pulledForeignKeyDetail))
                {
                    continue;
                }

                var foreignKeyAttribute = foreignKey.GetCustomAttribute<ForeignKeyAttribute>();
                var isList = foreignKey.PropertyType.IsList();
                var fkPropertyType = isList
                    ? foreignKey.PropertyType.GetGenericArguments()[0]
                    : foreignKey.PropertyType;
                var tableName = GetTableName(fkPropertyType);

                var joinString = isList
                    ? string.Format(" LEFT JOIN [{0}] As [{1}] On [{1}].[{2}] = [{3}].[{4}]",
                        tableName, foreignKey.Name, foreignKeyAttribute.ForeignKeyColumnName, GetTableName(type),
                        GetPrimaryKeys(type).First().Name)
                    : string.Format(" INNER JOIN [{0}] As [{1}] On [{1}].[{2}] = [{3}].[{4}]",
                        tableName, foreignKey.Name, GetPrimaryKeys(fkPropertyType).First().Name, GetTableName(type),
                        foreignKeyAttribute.ForeignKeyColumnName);

                var schematic = new ObjectSchematic
                {
                    Type = fkPropertyType,
                    ParentType = type,
                    IsList = isList,
                    ListType = isList ? foreignKey.PropertyType : null,
                    PropertyName = foreignKey.Name,
                    TableAlias = foreignKey.Name,
                    TableName = tableName,
                    ChildTypes = new List<ObjectSchematic>(),
                    JoinString = joinString,
                    PrimaryKeyDatabaseNames = _primaryKeyColumnNamesWithTableName(fkPropertyType),
                };

                if (!lasyLoad)
                {
                    _loadColumnSchematics(schematic, fkPropertyType, foreignKey.Name);
                }

                _getChildren(schematic.ChildTypes, fkPropertyType, resultingTypes, viewId, lasyLoad, foreignKey.Name);

                resultingTypes.Add(pulledForeignKeyDetail);

                result.Add(schematic);

                if (HasForeignKeys(fkPropertyType))
                {
                    _addForeignKeyTypesRecursive(result, fkPropertyType, resultingTypes, viewId, lasyLoad);
                }
            }
        }

        private static void _loadColumnSchematics(ObjectSchematic schematic, Type type, string tableName)
        {
            foreach (
                var item in
                    type.GetProperties().Where(w => w.GetCustomAttribute<NonSelectableAttribute>() == null))
            {
                schematic.ColumnSchematics.Add(new SqlColumnSchematic(tableName, GetColumnName(item),
                    type));
            }
        }

        private static void _getChildren(List<ObjectSchematic> result, Type type, List<PulledForeignKeyDetail> resultingTypes, string viewId, bool lazyLoad, string tableNameFromProperty)
        {
            var foreignKeys = GetForeignKeys(type, viewId);

            foreach (var foreignKey in foreignKeys)
            {
                var foreignKeyAttribute = foreignKey.GetCustomAttribute<ForeignKeyAttribute>();
                var isList = foreignKey.PropertyType.IsList();
                var fkPropertyType = isList
                    ? foreignKey.PropertyType.GetGenericArguments()[0]
                    : foreignKey.PropertyType;
                var tableName = GetTableName(fkPropertyType);

                var joinString = isList
                    ? string.Format(" LEFT JOIN [{0}] As [{1}] On [{1}].[{2}] = [{3}].[{4}]",
                        tableName, foreignKey.Name, foreignKeyAttribute.ForeignKeyColumnName, tableNameFromProperty,
                        GetPrimaryKeys(type).First().Name)
                    : string.Format(" INNER JOIN [{0}] As [{1}] On [{1}].[{2}] = [{3}].[{4}]",
                        tableName, foreignKey.Name, GetPrimaryKeys(fkPropertyType).First().Name, tableNameFromProperty,
                        foreignKeyAttribute.ForeignKeyColumnName);

                var schematic = new ObjectSchematic
                {
                    Type = fkPropertyType,
                    ParentType = type,
                    IsList = isList,
                    ListType = isList ? foreignKey.PropertyType : null,
                    PropertyName = foreignKey.Name,
                    TableAlias = foreignKey.Name,
                    TableName = GetTableName(fkPropertyType),
                    ChildTypes = new List<ObjectSchematic>(),
                    JoinString = joinString,
                    PrimaryKeyDatabaseNames = _primaryKeyColumnNamesWithTableName(fkPropertyType),
                };

                // load columns here if not lazy loading
                if (!lazyLoad)
                {
                    _loadColumnSchematics(schematic, fkPropertyType, foreignKey.Name);
                }

                result.Add(schematic);

                resultingTypes.Add(new PulledForeignKeyDetail(foreignKey));

                if (HasForeignKeys(fkPropertyType))
                {
                    _getChildren(schematic.ChildTypes, fkPropertyType, resultingTypes, viewId, lazyLoad, tableNameFromProperty);
                }
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
                : string.Format("{0}.[{1}]", linkedserverattribute.FormattedLinkedServerText, tableName);
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

        public static string[] GetPrimaryKeyNames(Type type)
        {
            return
                GetPrimaryKeys(type)
                    .Select(
                        w =>
                            w.GetCustomAttribute<ColumnAttribute>() == null
                                ? w.Name
                                : w.GetCustomAttribute<ColumnAttribute>().Name).ToArray();
        }

        public static PropertyInfo GetPrimaryKeyByName(string name, Type type)
        {
            return GetPrimaryKeys(type).FirstOrDefault(w => String.Equals(w.Name, name, StringComparison.CurrentCultureIgnoreCase));
        }

        public static List<PropertyInfo> GetForeignKeys(object entity, string viewId = null)
        {
            return GetForeignKeys(entity.GetType(), viewId);
        }

        public static List<PropertyInfo> GetForeignKeys<T>(string viewId = null)
        {
            return GetForeignKeys(typeof(T), viewId);
        }

        public static List<PropertyInfo> GetForeignKeys(Type entityType, string viewId = null)
        {
            return string.IsNullOrWhiteSpace(viewId)
                ? entityType.GetProperties().Where(w =>
                    w.GetCustomAttribute<ForeignKeyAttribute>() != null).ToList()
                : entityType.GetProperties().Where(w =>
                    w.GetCustomAttribute<ForeignKeyAttribute>() != null
                    && w.PropertyType.GetCustomAttribute<ViewAttribute>() != null
                    && w.PropertyType.GetCustomAttribute<ViewAttribute>().ViewIds.Contains(viewId)).ToList();
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
            return entityType.GetProperties().Where(w => w.GetCustomAttribute<UnmappedAttribute>() == null && w.GetCustomAttribute<AutoLoadKeyAttribute>() == null).ToList();
        }

        public static bool IsPartOfView(Type tabletype, string viewId)
        {
            var viewAttribute = tabletype.GetCustomAttribute<ViewAttribute>();

            return viewAttribute != null && viewAttribute.ViewIds.Contains(viewId);
        }

        public static List<JoinColumnPair> GetAllForeignKeysAndPseudoKeys(Type type, Guid expressionQueryId, string viewId)
        {
            var autoLoadProperties = string.IsNullOrWhiteSpace(viewId)
                ? type.GetProperties().Where(w => w.GetCustomAttribute<AutoLoadKeyAttribute>() != null)
                : type.GetProperties()
                    .Where(
                        w =>
                            w.GetCustomAttribute<AutoLoadKeyAttribute>() != null &&
                            w.PropertyType.GetCustomAttribute<ViewAttribute>() != null &&
                            w.PropertyType.GetCustomAttribute<ViewAttribute>().ViewIds.Contains(viewId));

            return (from property in autoLoadProperties
                let fkAttribute = property.GetCustomAttribute<ForeignKeyAttribute>()
                let pskAttribute = property.GetCustomAttribute<PseudoKeyAttribute>()
                select new JoinColumnPair
                {
                    ChildColumn =
                        new PartialColumn(expressionQueryId, property.GetPropertyType(),
                            fkAttribute != null
                                ? property.PropertyType.IsList()
                                    ? fkAttribute.ForeignKeyColumnName
                                    : GetPrimaryKeys(property.PropertyType).First().Name
                                : pskAttribute.ChildTableColumnName),
                    ParentColumn =
                        new PartialColumn(expressionQueryId, type,
                            fkAttribute != null
                                ? property.PropertyType.IsList()
                                    ? GetPrimaryKeys(type).First().Name
                                    : fkAttribute.ForeignKeyColumnName
                                : pskAttribute.ParentTableColumnName),
                    JoinType = property.PropertyType.IsList() ? JoinType.Left : JoinType.Inner,
                    JoinPropertyName = property.Name,
                    FromType = property.PropertyType
                }).ToList();
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

        private static string[] _primaryKeyColumnNamesWithTableName(Type type)
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

                result[i] = columnAttribute == null ? propertyInfo.Name : columnAttribute.Name;
            }

            return result;
        }

        public static bool IsPrimaryKey(Type type, string columnName)
        {
            return columnName.ToUpper() == "ID" ||
                   type.GetProperty(columnName).GetCustomAttribute<KeyAttribute>() != null;
        }
    }
}
