/*
 * OR-M Data Entities v3.0
 * License: The MIT License (MIT)
 * Code: https://github.com/jdemeuse1204/OR-M-Data-Entities
 * Email: james.demeuse@gmail.com
 * Copyright (c) 2014 James Demeuse
 */

using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using OR_M_Data_Entities.Enumeration;
using OR_M_Data_Entities.Expressions.Query.Columns;
using OR_M_Data_Entities.Expressions.Resolution.Join;
using OR_M_Data_Entities.Mapping;
using OR_M_Data_Entities.Mapping.Base;

// ReSharper disable once CheckNamespace
namespace OR_M_Data_Entities
{
    public static class SchemaExtensions
    {
        public static bool IsPrimaryKey(this MemberInfo column)
        {
            return column.Name.ToUpper() == "ID"
                || column.GetColumnName().ToUpper() == "ID"
                || column.GetCustomAttribute<KeyAttribute>() != null;
        }

        public static string GetTableName(this object entity)
        {
            return entity.GetType().GetTableName();
        }

        public static string GetTableName(this Type type)
        {
            // check for table name attribute
            var tableAttribute = type.GetCustomAttribute<TableAttribute>();

            return tableAttribute == null ? type.Name : tableAttribute.Name;
        }

        public static string GetTableNameWithLinkedServer(this object entity)
        {
            return entity.GetType().GetTableNameWithLinkedServer();
        }

        public static string GetTableNameWithLinkedServer(this Type type)
        {
            // check for table name attribute
            var tableAttribute = type.GetCustomAttribute<TableAttribute>();
            var linkedserverattribute = type.GetCustomAttribute<LinkedServerAttribute>();

            var tableName = tableAttribute == null ? type.Name : tableAttribute.Name;

            return linkedserverattribute == null
                ? tableName
                : string.Format("{0}.[{1}]", linkedserverattribute.FormattedLinkedServerText, tableName);
        }

        public static DbGenerationOption GetGenerationOption(this PropertyInfo column)
        {
            var dbGenerationColumn = column.GetCustomAttribute<DbGenerationOptionAttribute>();
            return dbGenerationColumn == null ? DbGenerationOption.IdentitySpecification : dbGenerationColumn.Option;
        }

        public static string GetColumnName(this MemberInfo column)
        {
            var columnAttribute = column.GetCustomAttribute<ColumnAttribute>();

            return columnAttribute == null ? column.Name : columnAttribute.Name;
        }

        public static string GetColumnName(this IEnumerable<PropertyInfo> properties, string propertyName)
        {
            var property = properties.FirstOrDefault(w => w.Name == propertyName);

            // property will be in list only if it has a custom attribute
            if (property == null) return propertyName;
            var columnAttribute = property.GetCustomAttribute<ColumnAttribute>();
            return columnAttribute == null ? propertyName : columnAttribute.Name;
        }

        public static List<PropertyInfo> GetPrimaryKeys(this object entity)
        {
            return entity.GetType().GetPrimaryKeys();
        }

        public static List<PropertyInfo> GetPrimaryKeys(this Type type)
        {
            var keyList = type.GetProperties().Where(w =>
               (w.GetCustomAttributes<SearchablePrimaryKeyAttribute>() != null
               && w.GetCustomAttributes<SearchablePrimaryKeyAttribute>().Any(x => x.IsPrimaryKey))
               || (w.Name.ToUpper() == "ID")).ToList();

            if (keyList.Count != 0)
            {
                return keyList;
            }

            throw new Exception(string.Format("Cannot find PrimaryKey(s) for type of {0}", type.Name));
        }

        public static string[] GetPrimaryKeyNames(this Type type)
        {
            return
                GetPrimaryKeys(type)
                    .Select(
                        w =>
                            w.GetCustomAttribute<ColumnAttribute>() == null
                                ? w.Name
                                : w.GetCustomAttribute<ColumnAttribute>().Name).ToArray();
        }

        public static List<PropertyInfo> GetForeignKeys(this object entity, string viewId = null)
        {
            return entity.GetType().GetForeignKeys(viewId);
        }

        public static ForeignKeyAttribute FindForeignKeyAttribute(this Type type, string propertyName)
        {
            return type.GetProperty(propertyName).GetCustomAttribute<ForeignKeyAttribute>();
        }

        public static List<PropertyInfo> GetForeignKeys(this Type entityType, string viewId = null)
        {
            return string.IsNullOrWhiteSpace(viewId)
                ? entityType.GetProperties().Where(w =>
                    w.GetCustomAttribute<ForeignKeyAttribute>() != null).ToList()
                : entityType.GetProperties().Where(w =>
                    w.GetCustomAttribute<ForeignKeyAttribute>() != null
                    && w.PropertyType.GetCustomAttribute<ViewAttribute>() != null
                    && w.PropertyType.GetCustomAttribute<ViewAttribute>().ViewIds.Contains(viewId)).ToList();
        }

        public static List<ForeignKeyAttribute> GetForeignKeyAttributes(this Type entityType)
        {
            return entityType.GetProperties().Where(w =>
               w.GetCustomAttribute<ForeignKeyAttribute>() != null).Select(w => w.GetCustomAttribute<ForeignKeyAttribute>()).ToList();
        }

        public static List<ForeignKeyAttribute> GetForeignKeyAttributes(this object entity)
        {
            return entity.GetType().GetForeignKeyAttributes();
        }

        public static List<ForeignKeyAttribute> GetForeignKeyAttributes<T>(this T entity)
        {
            return typeof(T).GetForeignKeyAttributes();
        }

        public static bool HasForeignKeys(this Type entityType)
        {
            return entityType.GetProperties().Count(w => w.GetCustomAttribute<ForeignKeyAttribute>() != null) > 0;
        }

        public static bool HasForeignKeys(TypeInfo entityType)
        {
            return entityType.UnderlyingSystemType.HasForeignKeys();
        }

        public static bool HasForeignKeys(this object entity)
        {
            return entity.GetType().HasForeignKeys();
        }

        public static bool IsTableReadOnly(this Type type)
        {
            return type.GetCustomAttribute<ReadOnlyAttribute>() != null;
        }

        public static bool IsTableReadOnly(this object entity)
        {
            return entity.GetType().IsTableReadOnly();
        }

        public static bool HasForeignListKeys(this object entity)
        {
            return entity.GetForeignKeys().Any(w => w.PropertyType.IsList());
        }

        public static bool HasForeignListKeys(this Type type)
        {
            return type.GetForeignKeys().Any(w => w.PropertyType.IsList());
        }

        public static bool HasForeignNonListKeys(this object entity)
        {
            return entity.GetForeignKeys().Any(w => !w.PropertyType.IsList());
        }

        public static bool HasForeignNonListKeys(this Type type)
        {
            return type.GetForeignKeys().Any(w => !w.PropertyType.IsList());
        }

        public static List<PropertyInfo> GetTableFields(this object entity)
        {
            return entity.GetType().GetTableFields();
        }

        public static List<PropertyInfo> GetTableFields(this Type entityType)
        {
            return entityType.GetProperties().Where(w => w.GetCustomAttribute<UnmappedAttribute>() == null && w.GetCustomAttribute<AutoLoadKeyAttribute>() == null).ToList();
        }

        public static bool IsPartOfView(this Type tabletype, string viewId)
        {
            var viewAttribute = tabletype.GetCustomAttribute<ViewAttribute>();

            return viewAttribute != null && viewAttribute.ViewIds.Contains(viewId);
        }

        public static List<JoinColumnPair> GetAllForeignKeysAndPseudoKeys(this Type type, Guid expressionQueryId, string viewId)
        {
            var autoLoadProperties = string.IsNullOrWhiteSpace(viewId)
                ? type.GetProperties().Where(w => w.GetCustomAttribute<AutoLoadKeyAttribute>() != null)
                : type.GetProperties()
                    .Where(
                        w => w.GetCustomAttribute<AutoLoadKeyAttribute>() != null &&
                            w.GetPropertyType().GetCustomAttribute<ViewAttribute>() != null &&
                             w.GetPropertyType().GetCustomAttribute<ViewAttribute>().ViewIds.Contains(viewId));

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
                    JoinType =
                        property.PropertyType.IsList()
                            ? JoinType.Left
                            : type.GetProperty(fkAttribute != null ? fkAttribute.ForeignKeyColumnName : pskAttribute.ParentTableColumnName).PropertyType.IsNullable()
                                ? JoinType.Left
                                : JoinType.Inner,
                    JoinPropertyName = property.Name,
                    FromType = property.PropertyType
                }).ToList();
        }

        public static KeyValuePair<string, IEnumerable<string>> GetSelectAllFieldsAndTableName(this Type tableType)
        {
            var table = GetTableName(tableType);
            var fields = GetTableFields(tableType).Select(w => w.GetCustomAttribute<ColumnAttribute>() != null ? w.GetCustomAttribute<ColumnAttribute>().Name : w.Name);

            return new KeyValuePair<string, IEnumerable<string>>(table, fields);
        }

        public static bool IsPrimaryKey(this Type type, string columnName)
        {
            return columnName.ToUpper() == "ID" ||
                   type.GetProperty(columnName).GetCustomAttribute<KeyAttribute>() != null;
        }

        public static bool IsColumn(this PropertyInfo info)
        {
            var attributes = info.GetCustomAttributes();

            var isNonSelectable = attributes.Any(w => w is NonSelectableAttribute);
            var isPrimaryKey = attributes.Any(w => w is SearchablePrimaryKeyAttribute);
            var hasAttributes = attributes != null && attributes.Any();

            return (hasAttributes && (isPrimaryKey || !isNonSelectable)) || !hasAttributes;
        }

        public static bool HasPrimaryKeysOnly(this object entity)
        {
            var properties = entity.GetType().GetProperties();
            return properties.Count(w => w.IsColumn()) == properties.Count(w => w.IsPrimaryKey());
        }
    }
}
