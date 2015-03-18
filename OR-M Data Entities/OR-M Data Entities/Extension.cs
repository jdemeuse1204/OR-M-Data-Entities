/*
 * OR-M Data Entities v1.2.0
 * License: The MIT License (MIT)
 * Code: https://github.com/jdemeuse1204/OR-M-Data-Entities
 * (c) 2015 James Demeuse
 */

using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Dynamic;
using System.Linq;
using System.Reflection;
using OR_M_Data_Entities.Commands;
using OR_M_Data_Entities.Commands.Support;
using OR_M_Data_Entities.Data;
using OR_M_Data_Entities.Expressions.Resolver;
using OR_M_Data_Entities.Mapping;

namespace OR_M_Data_Entities
{
	public static class Extension
	{
		/// <summary>
		/// Converts a SqlDataReader to an object.  The return column names must match the properties names for it to work
		/// </summary>
		/// <typeparam name="T"></typeparam>
		/// <param name="reader"></param>
		/// <returns></returns>
		private static T _toObject<T>(this SqlDataReader reader)
		{
            if (!reader.HasRows) return default(T);

		    if (typeof (T).IsValueType
				|| typeof(T) == typeof(string))
		    {
		        return (T)reader[0];
		    }

            if (typeof(T) == typeof(IDynamicMetaObjectProvider))
		    {
		        return reader.ToObject();
		    }

			// Create instance
			var obj = Activator.CreateInstance<T>();

			// find any unmapped attributes
            var properties = obj.GetType().GetProperties().Where(w => w.GetCustomAttribute<UnmappedAttribute>() == null 
                && w.GetCustomAttribute<AutoLoadAttribute>() == null).ToList();

			// find any columns that have the column name attribute on them,
			// we need to swtich the column name to the one in the property
			var columnRenameProperties = obj.GetType().GetProperties().Where(w => w.GetCustomAttribute<ColumnAttribute>() != null).Select(w => w.Name).ToList();

		    for (var i = 0; i < properties.Count; i++)
		    {
		        var property = properties[i];
		        var columnName = property.Name;

		        if (columnRenameProperties.Contains(columnName))
		        {
		            columnName = property.GetCustomAttribute<ColumnAttribute>().Name;
		        }

		        var dbValue = reader[columnName];
		        DatabaseEntity.SetPropertyValue(obj, property.Name, dbValue is DBNull ? null : dbValue);
		    }

		    return obj;
		}

	    public static T ToObject<T>(this SqlDataReader reader, string connectionString)
	    {
	        var result = _toObject<T>(reader);

	        if (result == null) return result;

	        using (var context = new DbSqlContext(connectionString))
	        {
	            _recursiveActivator(result, context);
	        }

	        return result;
	    }

        private static void _recursiveActivator(object parent, DbSqlContext context)
        {
            var autoLoads = parent.GetType().GetProperties().Where(w => w.GetCustomAttribute<AutoLoadAttribute>() != null).ToList();

            foreach (var autoLoad in autoLoads)
            {
                var childInstance = Activator.CreateInstance(autoLoad.PropertyType);

                if (ExpressionTypeTransform.IsList(childInstance))
                {
                    var listItemType = childInstance.GetType().GetGenericArguments()[0];
                    var listItemTable = DatabaseSchemata.GetTableName(listItemType);
                    var listItemProperties = listItemType.GetProperties();
                    var listForeignKeys = listItemProperties.Where(w => w.GetCustomAttribute<ForeignKeyAttribute>() != null
                        && w.GetCustomAttribute<ForeignKeyAttribute>().ParentTableType == parent.GetType()).ToList();

                    var listSelectBuilder = new SqlQueryBuilder();
                    listSelectBuilder.Table(listItemTable);
                    listSelectBuilder.SelectAll(listItemType);

                    foreach (var item in listForeignKeys)
                    {
                        var columnName = DatabaseSchemata.GetColumnName(item);
                        var foreignKey = item.GetCustomAttribute<ForeignKeyAttribute>();
                        var compareValue = parent.GetType().GetProperty(foreignKey.ParentPropertyName).GetValue(parent);
                        listSelectBuilder.AddWhere(listItemTable, columnName, ComparisonType.Equals, compareValue);
                    }

                    var listMethod = context.GetType().GetMethods().First(w => w.Name == "ExecuteQuery"
                        && w.GetParameters().Any()
                        && w.GetParameters()[0].ParameterType == typeof(ISqlBuilder));

                    var genericListMethod = listMethod.MakeGenericMethod(new[] { listItemType });
                    var listResult = genericListMethod.Invoke(context, new object[] { listSelectBuilder });
                    var allResults = (listResult as dynamic).All();

                    foreach (var item in allResults)
                    {
                        _recursiveActivator(item, context);
                        (childInstance as dynamic).Add(item);
                    }

                    autoLoad.SetValue(parent, childInstance, null);
                    continue;
                }

                var itemType = childInstance.GetType();
                var itemTable = DatabaseSchemata.GetTableName(itemType);
                var itemProperties = itemType.GetProperties();
                var foreignKeys = itemProperties.Where(w => w.GetCustomAttribute<ForeignKeyAttribute>() != null
                    && w.GetCustomAttribute<ForeignKeyAttribute>().ParentTableType == parent.GetType()).ToList();

                var builder = new SqlQueryBuilder();
                builder.Table(itemTable);
                builder.SelectAll(itemType);

                foreach (var item in foreignKeys)
                {
                    var columnName = DatabaseSchemata.GetColumnName(item);
                    var foreignKey = item.GetCustomAttribute<ForeignKeyAttribute>();
                    var compareValue = parent.GetType().GetProperty(foreignKey.ParentPropertyName).GetValue(parent);
                    builder.AddWhere(itemTable, columnName, ComparisonType.Equals, compareValue);
                }

                var method = context.GetType().GetMethods().First(w => w.Name == "ExecuteQuery"
                    && w.GetParameters().Any()
                    && w.GetParameters()[0].ParameterType == typeof(ISqlBuilder));

                var genericMethod = method.MakeGenericMethod(new[] { itemType });
                var query = genericMethod.Invoke(context, new object[] { builder });
                var result = (query as dynamic).Select();

                context.Disconnect();

                autoLoad.SetValue(parent, result, null);

                if (result != null)
                {
                    _recursiveActivator(result, context);
                }
            }
        }

		public static bool IsNumeric(this object o)
		{
			var result = 0L;

			return long.TryParse(o.ToString(), out result);
		}

		/// <summary>
		/// Turns the DataReader into an object and converts the types for you
		/// </summary>
		/// <param name="reader"></param>
		/// <returns></returns>
		public static dynamic ToObject(this SqlDataReader reader)
		{
			if (!reader.HasRows)
			{
				return null;
			}

			var result = new ExpandoObject() as IDictionary<string, Object>;

			var rec = (IDataRecord)reader;

			for (var i = 0; i < rec.FieldCount; i++)
			{
				result.Add(rec.GetName(i), rec.GetValue(i));
			}

			return result;
		}
	}

	public static class StringExtension
	{
		public static string ReplaceFirst(this string text, string search, string replace)
		{
			var pos = text.IndexOf(search);

			if (pos < 0)
			{
				return text;
			}

			return text.Substring(0, pos) + replace + text.Substring(pos + search.Length);
		}
	}
}
