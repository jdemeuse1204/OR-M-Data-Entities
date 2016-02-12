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

        public static bool HasForeignKeys(this Type entityType)
        {
            return entityType.GetProperties().Count(w => w.GetCustomAttribute<ForeignKeyAttribute>() != null) > 0;
        }

        public static bool HasForeignKeys(this object entity)
        {
            return entity.GetType().HasForeignKeys();
        }

        public static bool IsPartOfView(this Type tabletype, string viewId)
        {
            var viewAttribute = tabletype.GetCustomAttribute<ViewAttribute>();

            return viewAttribute != null && viewAttribute.ViewIds.Contains(viewId);
        }
    }
}
