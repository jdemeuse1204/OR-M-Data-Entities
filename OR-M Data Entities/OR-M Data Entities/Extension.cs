/*
 * OR-M Data Entities v1.2.0
 * License: The MIT License (MIT)
 * Code: https://github.com/jdemeuse1204/OR-M-Data-Entities
 * (c) 2015 James Demeuse
 */

using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Data.SqlClient;
using System.Dynamic;
using System.Linq;
using System.Reflection;
using OR_M_Data_Entities.Data;
using OR_M_Data_Entities.Mapping;
using OR_M_Data_Entities.Mapping.Base;

namespace OR_M_Data_Entities
{
	public static class Extension
	{
        public static string GetNextParameter(this Dictionary<string,object> parameters)
        {
            return string.Format("@Param{0}", parameters.Count);
        }

        public static bool IsList(this object o)
        {
            return o is IList &&
                   o.GetType().IsGenericType &&
                   o.GetType().GetGenericTypeDefinition().IsAssignableFrom(typeof(List<>));
        }

        public static bool IsList(this Type type)
        {
            return type.IsGenericType &&
                   type.GetGenericTypeDefinition().IsAssignableFrom(typeof(List<>));
        }

	    /// <summary>
	    /// Converts a SqlDataReader to an object.  The return column names must match the properties names for it to work
	    /// </summary>
	    /// <typeparam name="T"></typeparam>
	    /// <param name="reader"></param>
	    /// <param name="dataReaderLoadType"></param>
	    /// <returns></returns>
	    public static T ToObject<T>(this SqlDataReader reader)
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

            return reader.GetObjectFromReader<T>(DatabaseSchemata.HasForeignKeys<T>() ? DataReaderLoadType.TableColumnLoad : DataReaderLoadType.ColumnLoad);
		}

	    private static T GetObjectFromReader<T>(this SqlDataReader reader, DataReaderLoadType dataReaderLoadType)
	    {
            // Create instance
            var instance = Activator.CreateInstance<T>();

	        var tableName = DatabaseSchemata.GetTableName<T>();

            // find any unmapped attributes
            var properties = typeof(T).GetProperties().Where(w => w.GetCustomAttribute<NonSelectableAttribute>() == null).ToList();

            foreach (var property in properties)
            {
                var columnAttribute = property.GetCustomAttribute<ColumnAttribute>();

                // need to select by tablename and columnname because of joins.  Column names cannot be ambiguous
                var dbValue = reader[dataReaderLoadType == DataReaderLoadType.ColumnLoad ?
                    (columnAttribute != null ? columnAttribute.Name : property.Name) :  
                    tableName + (columnAttribute != null ? columnAttribute.Name : property.Name)];

                instance._setPropertyValue(property, dbValue is DBNull ? null : dbValue);
            }

	        return instance;
	    }

        private static void _setPropertyValue(this object entity, PropertyInfo property, object value)
        {
            var propertyType = property.PropertyType;

            //Nullable properties have to be treated differently, since we 
            //  use their underlying property to set the value in the object
            if (propertyType.IsGenericType
                && propertyType.GetGenericTypeDefinition() == typeof(Nullable<>))
            {
                //if it's null, just set the value from the reserved word null, and return
                if (value == null)
                {
                    property.SetValue(entity, null, null);
                    return;
                }

                //Get the underlying type property instead of the nullable generic
                propertyType = new NullableConverter(property.PropertyType).UnderlyingType;
            }

            //use the converter to get the correct value
            property.SetValue(
                entity,
                propertyType.IsEnum ? Enum.ToObject(propertyType, value) : Convert.ChangeType(value, propertyType),
                null);
        }

        //private static void _recursiveActivator(object parent, DbSqlContext context)
        //{
        //    var autoLoads = parent.GetType().GetProperties().Where(w => w.GetCustomAttribute<AutoLoadAttribute>() != null).ToList();

        //    foreach (var autoLoad in autoLoads)
        //    {
        //        var childInstance = Activator.CreateInstance(autoLoad.PropertyType);

        //        if (ExpressionTypeTransform.IsList(childInstance))
        //        {
        //            var listItemType = childInstance.GetType().GetGenericArguments()[0];
        //            var listItemTable = DatabaseSchemata.GetTableName(listItemType);
        //            var listItemProperties = listItemType.GetProperties();
        //            var listForeignKeys = listItemProperties.Where(w => w.GetCustomAttribute<ForeignKeyAttribute>() != null
        //                && w.GetCustomAttribute<ForeignKeyAttribute>().ParentTableType == parent.GetType()).ToList();

        //            var listSelectBuilder = new SqlQueryBuilder();
        //            listSelectBuilder.Table(listItemTable);
        //            listSelectBuilder.SelectAll(listItemType);

        //            foreach (var item in listForeignKeys)
        //            {
        //                var columnName = DatabaseSchemata.GetColumnName(item);
        //                var foreignKey = item.GetCustomAttribute<ForeignKeyAttribute>();
        //                var compareValue = parent.GetType().GetProperty(foreignKey.ParentPropertyName).GetValue(parent);
        //                listSelectBuilder.AddWhere(listItemTable, columnName, ComparisonType.Equals, compareValue);
        //            }

        //            var listMethod = context.GetType().GetMethods().First(w => w.Name == "ExecuteQuery"
        //                && w.GetParameters().Any()
        //                && w.GetParameters()[0].ParameterType == typeof(ISqlBuilder));

        //            var genericListMethod = listMethod.MakeGenericMethod(new[] { listItemType });
        //            var listResult = genericListMethod.Invoke(context, new object[] { listSelectBuilder });
        //            var allResults = (listResult as dynamic).All();

        //            (listResult as dynamic).Dispose();
        //            context.Disconnect();

        //            foreach (var item in allResults)
        //            {
        //                _recursiveActivator(item, context);
        //                (childInstance as dynamic).Add(item);
        //            }

        //            autoLoad.SetValue(parent, childInstance, null);
        //            continue;
        //        }

        //        var itemType = childInstance.GetType();
        //        var itemTable = DatabaseSchemata.GetTableName(itemType);
        //        var itemProperties = itemType.GetProperties();
        //        var foreignKeys = itemProperties.Where(w => w.GetCustomAttribute<ForeignKeyAttribute>() != null
        //            && w.GetCustomAttribute<ForeignKeyAttribute>().ParentTableType == parent.GetType()).ToList();

        //        var builder = new SqlQueryBuilder();
        //        builder.Table(itemTable);
        //        builder.SelectAll(itemType);

        //        foreach (var item in foreignKeys)
        //        {
        //            var columnName = DatabaseSchemata.GetColumnName(item);
        //            var foreignKey = item.GetCustomAttribute<ForeignKeyAttribute>();
        //            var compareValue = parent.GetType().GetProperty(foreignKey.ParentPropertyName).GetValue(parent);
        //            builder.AddWhere(itemTable, columnName, ComparisonType.Equals, compareValue);
        //        }

        //        var method = context.GetType().GetMethods().First(w => w.Name == "ExecuteQuery"
        //            && w.GetParameters().Any()
        //            && w.GetParameters()[0].ParameterType == typeof(ISqlBuilder));

        //        var genericMethod = method.MakeGenericMethod(new[] { itemType });
        //        var query = genericMethod.Invoke(context, new object[] { builder });
        //        var result = (query as dynamic).Select();

        //        (query as dynamic).Dispose();
        //        context.Disconnect();

        //        autoLoad.SetValue(parent, result, null);

        //        if (result != null)
        //        {
        //            _recursiveActivator(result, context);
        //        }
        //    }
        //}

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
