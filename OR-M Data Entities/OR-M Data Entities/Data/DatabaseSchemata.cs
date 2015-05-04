/*
 * OR-M Data Entities v1.1.0
 * License: The MIT License (MIT)
 * Code: https://github.com/jdemeuse1204/OR-M-Data-Entities
 * (c) 2015 James Demeuse
 */

using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using OR_M_Data_Entities.Infrastructure;
using OR_M_Data_Entities.Mapping;

namespace OR_M_Data_Entities.Data
{
    public static class DatabaseSchemata
    {
        public static bool IsPrimaryKey(PropertyInfo column)
        {
            return column.Name.ToUpper() == "ID"
                || GetColumnName(column).ToUpper() == "ID"
                || column.GetCustomAttribute<KeyAttribute>() != null;
        }

        public static SqlStatementTableDetails GetTableDetails(object entity)
        {
            return GetTableDetails(entity.GetType());
        }

        public static SqlStatementTableDetails GetTableDetails(Type type)
        {
            return new SqlStatementTableDetails(type);
        }

        public static SqlStatementTableDetails GetTableDetails<T>()
        {
            return GetTableDetails(typeof(T));
        }

        public static DbGenerationOption GetGenerationOption(PropertyInfo column)
        {
            var dbGenerationColumn = column.GetCustomAttribute<DbGenerationOptionAttribute>();
            return dbGenerationColumn == null ? DbGenerationOption.IdentitySpecification : dbGenerationColumn.Option;
        }

        public static string GetColumnName(PropertyInfo column)
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
            var keyList = entity.GetType().GetProperties().Where(w =>
               (w.GetCustomAttribute<KeyAttribute>() != null) ||
               (w.GetCustomAttribute<ColumnAttribute>() != null && w.GetCustomAttribute<ColumnAttribute>().IsPrimaryKey) ||
               (w.Name.ToUpper() == "ID")).ToList();

            if (keyList.Count != 0)
            {
                return keyList;
            }

            throw new Exception("Cannot find PrimaryKey(s)");
        }

        public static List<PropertyInfo> GetTableFields(object entity)
        {
            return GetTableFields(entity.GetType());
        }

        public static List<PropertyInfo> GetTableFields(Type type)
        {
            return type.GetProperties().Where(w => w.GetCustomAttribute<UnmappedAttribute>() == null).ToList();
        } 
    }
}
