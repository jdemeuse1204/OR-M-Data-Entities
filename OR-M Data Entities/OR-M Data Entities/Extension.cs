/*
 * OR-M Data Entities v1.2.0
 * License: The MIT License (MIT)
 * Code: https://github.com/jdemeuse1204/OR-M-Data-Entities
 * Copyright (c) 2015 James Demeuse
 */

using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Data.SqlClient;
using System.Dynamic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using OR_M_Data_Entities.Data;
using OR_M_Data_Entities.Data.Definition;
using OR_M_Data_Entities.Expressions;
using OR_M_Data_Entities.Expressions.Resolution;
using OR_M_Data_Entities.Mapping;
using OR_M_Data_Entities.Mapping.Base;

namespace OR_M_Data_Entities
{
    public static class ExpressionQueryExtensions
    {
        public static ExpressionQuery<TSource> Where<TSource>(this ExpressionQuery<TSource> source, Expression<Func<TSource, bool>> expression)
        {
            SqlExpressionWhereResolver.Resolve(expression, source.Query);

            return source;
        }

        public static TSource First<TSource>(this ExpressionQuery<TSource> source)
        {
            source.Query.Take = 1;

            return default(TSource);
        }

        public static TSource First<TSource>(this ExpressionQuery<TSource> source, Expression<Func<TSource, bool>> expression)
        {
            source.Query.Take = 1;

            source.Where(expression);

            return default(TSource);
        }

        public static TSource FirstOrDefault<TSource>(this ExpressionQuery<TSource> source)
        {
            source.Query.Take = 1;
            source.Query.IsFirstOrDefault = true;

            return default(TSource);
        }

        public static TSource FirstOrDefault<TSource>(this ExpressionQuery<TSource> source, Expression<Func<TSource, bool>> expression)
        {
            source.Query.Take = 1;
            source.Query.IsFirstOrDefault = true;
            source.Where(expression);

            var sql = source.Query.ToSql();

            if (sql != "")
            {

            }

            return default(TSource);
        }

        public static ExpressionQuery<TResult> InnerJoin<TOuter, TInner, TKey, TResult>(this ExpressionQuery<TOuter> outer,
            ExpressionQuery<TInner> inner, Expression<Func<TOuter, TKey>> outerKeySelector, Expression<Func<TInner, TKey>> innerKeySelector,
            Expression<Func<TOuter, TInner, TResult>> resultSelector)
        {
            var result = new ExpressionQuery<TResult>(outer.Query, outer.Context);

            //SqlExpressionSelectResolver.Resolve(selector, result.Query);

            return result;
        }

        public static ExpressionQuery<TResult> Select<TSource, TResult>(this ExpressionQuery<TSource> source,
            Expression<Func<TSource, int, TResult>> selector)
        {
            var result = new ExpressionQuery<TResult>(source.Query, source.Context);

            //SqlExpressionSelectResolver.Resolve(selector, result.Query);

            return result;
        }

        public static ExpressionQuery<TResult> Select<TSource, TResult>(this ExpressionQuery<TSource> source,
            Expression<Func<TSource, TResult>> selector)
        {
            var result = new ExpressionQuery<TResult>(source.Query, source.Context);

            SqlExpressionSelectResolver.Resolve(selector, result.Query);

            return result;
        }
    }

    #region helpers
    sealed class ObjectMapNode : IEquatable<ObjectMapNode>
    {
        public ObjectMapNode()
        {
            Children = new List<ObjectMapNode>();
            CurrentKeyHashCodeList = new List<int>();
        }

        public ObjectMapNode(int parentKey)
            : this()
        {
            ParentKey = parentKey;
        }

        public int ParentKey { get; set; }

        // current hashcodes
        public List<int> CurrentKeyHashCodeList { get; set; }

        public List<ObjectMapNode> Children { get; set; }

        public bool Equals(ObjectMapNode other)
        {
            //Check whether the compared object is null.
            if (Object.ReferenceEquals(other, null)) return false;

            //Check whether the compared object references the same data.
            if (Object.ReferenceEquals(this, other)) return true;

            //Check whether the products' properties are equal.
            return ParentKey == other.ParentKey;
        }

        // If Equals() returns true for a pair of objects 
        // then GetHashCode() must return the same value for these objects.
        public override int GetHashCode()
        {
            //Calculate the hash code for the product.
            return ParentKey.GetHashCode();
        }
    }
    #endregion

    public static class PeekDataReaderExtensions
    {
        public static dynamic ToObject(this PeekDataReader reader)
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

        public static T ToObject<T>(this PeekDataReader reader)
        {
            if (!reader.HasRows) return default(T);

            if (typeof(T).IsValueType
                || typeof(T) == typeof(string))
            {
                var data = reader[0];

                return data == DBNull.Value ? default(T) : (T)data;
            }

            if (typeof(T) == typeof(object))
            {
                return reader.ToObject();
            }

            return (reader.Query.IsLazyLoading || reader.Query == null) ? reader.GetObjectFromReader<T>() :
            reader.GetObjectFromReaderWithForeignKeys<T>();
        }

        #region Helpers
        private static T GetObjectFromReader<T>(this PeekDataReader reader)
        {
            // Create instance
            var instance = Activator.CreateInstance<T>();

            // find any unmapped attributes
            var properties = typeof(T).GetProperties().Where(w => w.GetCustomAttribute<NonSelectableAttribute>() == null).ToList();

            foreach (var property in properties)
            {
                var columnAttribute = property.GetCustomAttribute<ColumnAttribute>();

                // need to select by tablename and columnname because of joins.  Column names cannot be ambiguous
                var dbValue = reader[columnAttribute != null ? columnAttribute.Name : property.Name];

                instance.SetPropertyInfoValue(property, dbValue is DBNull ? null : dbValue);
            }

            return instance;
        }

        #region Load Object With Foreign Keys
        private static T GetObjectFromReaderUsingTableName<T>(this PeekDataReader reader)
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
                var dbValue = reader[tableName + (columnAttribute != null ? columnAttribute.Name : property.Name)];

                instance.SetPropertyInfoValue(property, dbValue is DBNull ? null : dbValue);
            }

            return instance;
        }

        private static bool LoadObjectWithForeignKeys(this PeekDataReader reader, object instance, string tableName)
        {
            try
            {
                // find any unmapped attributes
                var properties = instance.GetType().GetProperties().Where(w => w.GetCustomAttribute<NonSelectableAttribute>() == null).ToList();

                foreach (var property in properties)
                {
                    var columnAttribute = property.GetCustomAttribute<ColumnAttribute>();

                    // need to select by tablename and columnname because of joins.  Column names cannot be ambiguous
                    var dbValue = reader[tableName + (columnAttribute != null ? columnAttribute.Name : property.Name)];

                    instance.SetPropertyInfoValue(property, dbValue is DBNull ? null : dbValue);
                }

                return true;
            }
            catch
            {
                return false;
            }
        }

        private static T GetObjectFromReaderWithForeignKeys<T>(this PeekDataReader reader)
        {
            // Create instance
            var instance = Activator.CreateInstance<T>();
            var tableName = DatabaseSchemata.GetTableName<T>();
            var primaryKey = DatabaseSchemata.GetPrimaryKeys<T>().First();
            var foreignKeys = DatabaseSchemata.GetObjectSchematic(instance);
            var primaryKeyLookUpName = string.Format("{0}{1}", tableName, DatabaseSchemata.GetColumnName(primaryKey));
            var objectMapNodes = new List<ObjectMapNode>();

            // load the instance
            reader.LoadObjectWithForeignKeys(instance, tableName);

            var pkValue = instance.GetType().GetProperty(primaryKey.Name).GetValue(instance);

            _loadObjectWithForeignKeys(reader, instance, foreignKeys.ChildTypes, primaryKeyLookUpName, pkValue, objectMapNodes);

            return instance;
        }

        private static void _recursiveLoadWithForeignKeys(
            PeekDataReader reader,
            object childInstance,
            IEnumerable<ObjectSchematic> foreignKeys,
            List<ObjectMapNode> objectMapNodes,
            int lastCompositeKey)
        {
            foreach (var foreignKey in foreignKeys)
            {
                var property = childInstance.GetType().GetProperty(foreignKey.PropertyName);
                var propertyInstance = property.GetValue(childInstance);
                var currentCompositeKey = foreignKey.PrimaryKeyDatabaseNames.Sum(t => reader[t].GetHashCode());
                var node = new ObjectMapNode(lastCompositeKey);
                var index = objectMapNodes.IndexOf(node);
                var canAddObject = index == -1;

                // need to make sure the object has not been mapped
                if (canAddObject)
                {
                    // last key not in list, need to add it and current
                    node.CurrentKeyHashCodeList.Add(currentCompositeKey);
                    objectMapNodes.Add(node);
                }
                else
                {
                    node = objectMapNodes[index];
                    canAddObject = !node.CurrentKeyHashCodeList.Contains(currentCompositeKey);

                    if (canAddObject)
                    {
                        node.CurrentKeyHashCodeList.Add(currentCompositeKey);
                    }
                }

                if (foreignKey.IsList)
                {
                    var listItem = Activator.CreateInstance(foreignKey.Type);

                    if (propertyInstance == null)
                    {
                        propertyInstance = Activator.CreateInstance(foreignKey.ListType);
                        property.SetValue(childInstance, propertyInstance);

                        if (!reader.LoadObjectWithForeignKeys(listItem, foreignKey.PropertyName)) continue; // can only happen on a list because its one to many

                        _recursiveLoadWithForeignKeys(reader, listItem, foreignKey.ChildTypes, node.Children, currentCompositeKey);
                        propertyInstance.GetType().GetMethod("Add").Invoke(propertyInstance, new[] { listItem });
                    }
                    else
                    {
                        if (canAddObject)
                        {
                            if (!reader.LoadObjectWithForeignKeys(listItem, foreignKey.PropertyName)) continue; // can only happen on a list because its one to many

                            _recursiveLoadWithForeignKeys(reader, listItem, foreignKey.ChildTypes, node.Children, currentCompositeKey);
                            propertyInstance.GetType().GetMethod("Add").Invoke(propertyInstance, new[] { listItem });
                        }
                        else
                        {
                            // find last loaded item
                            var count = (int)propertyInstance.GetType().GetMethod("get_Count").Invoke(propertyInstance, null);

                            if (count != 0)
                            {
                                listItem = propertyInstance.GetType()
                                    .GetMethod("get_Item")
                                    .Invoke(propertyInstance, new object[] { count - 1 });

                                _recursiveLoadWithForeignKeys(reader, listItem, foreignKey.ChildTypes, node.Children, currentCompositeKey);
                            }
                        }
                    }

                    continue;
                }

                if (propertyInstance == null)
                {
                    propertyInstance = Activator.CreateInstance(foreignKey.Type);
                    property.SetValue(childInstance, propertyInstance);

                    reader.LoadObjectWithForeignKeys(propertyInstance, foreignKey.PropertyName);

                    _recursiveLoadWithForeignKeys(reader, propertyInstance, foreignKey.ChildTypes, node.Children, currentCompositeKey);
                }
                else
                {
                    _recursiveLoadWithForeignKeys(reader, propertyInstance, foreignKey.ChildTypes, node.Children, currentCompositeKey);
                }
            }
        }

        private static void _loadObjectWithForeignKeys(
            PeekDataReader reader,
            object instance,
            IEnumerable<ObjectSchematic> foreignKeys,
            string primaryKeyLookUpName,
            object pkValue,
            List<ObjectMapNode> objectMapNodes)
        {
            foreach (var foreignKey in foreignKeys)
            {
                var childInstance = Activator.CreateInstance(foreignKey.Type);
                var singleInstance = instance.GetType().GetProperty(foreignKey.PropertyName).GetValue(instance);
                var currentCompositeKey = foreignKey.PrimaryKeyDatabaseNames.Sum(t => reader[t].GetHashCode());
                var node = new ObjectMapNode(0);
                var index = objectMapNodes.IndexOf(node);

                if (singleInstance == null || singleInstance.IsList())
                {
                    if (!reader.LoadObjectWithForeignKeys(childInstance, foreignKey.PropertyName)) continue; // can only happen on a list because its one to many
                }

                if (index == -1)
                {
                    node.CurrentKeyHashCodeList.Add(currentCompositeKey);

                    objectMapNodes.Add(node);
                }
                else
                {
                    node = objectMapNodes[index];
                }

                if (foreignKey.IsList)
                {
                    var wasListInstanceCreated = false;

                    if (singleInstance == null)
                    {
                        wasListInstanceCreated = true;
                        singleInstance = Activator.CreateInstance(foreignKey.ListType); ;
                    }

                    // go down each FK tree and create the child instance
                    _recursiveLoadWithForeignKeys(reader, childInstance, foreignKey.ChildTypes, node.Children, currentCompositeKey);

                    singleInstance.GetType().GetMethod("Add").Invoke(singleInstance, new[] { childInstance });

                    if (wasListInstanceCreated)
                    {
                        instance.GetType().GetProperty(foreignKey.PropertyName).SetValue(instance, singleInstance);
                    }
                    continue;
                }

                _recursiveLoadWithForeignKeys(reader, singleInstance ?? childInstance, foreignKey.ChildTypes, node.Children, currentCompositeKey);

                if (singleInstance == null)
                {
                    instance.GetType().GetProperty(foreignKey.PropertyName).SetValue(instance, childInstance);
                }
            }

            // make sure we can peek
            if (!reader.Peek()) return;

            // if one column doesnt match then we do not have a match

            // TODO - Check for multiple keys!
            if (!pkValue.Equals(reader[primaryKeyLookUpName]))
            {
                return;
            }

            // read the next row
            reader.Read();

            _loadObjectWithForeignKeys(reader, instance, foreignKeys, primaryKeyLookUpName, pkValue, objectMapNodes);
        }
        #endregion


        #endregion
    }

    public static class PropertyInfoExtensions
    {
        public static Type GetPropertyType(this PropertyInfo info)
        {
            return info.PropertyType.IsList() ? info.PropertyType.GetGenericArguments()[0] : info.PropertyType;
        }
    }

    public static class TypeExtensions
    {
        public static bool IsDynamic(this Type type)
        {
            return type == typeof(IDynamicMetaObjectProvider);
        }
    }

    public static class SqlCommandExtensions
    {
        public static PeekDataReader ExecuteReaderWithPeeking(this SqlCommand cmd)
        {
            return new PeekDataReader(cmd);
        }

        public static PeekDataReader ExecuteReaderWithPeeking(this SqlCommand cmd, SqlQuery query)
        {
            return new PeekDataReader(cmd, query);
        }
    }

    public static class ListExtensions
    {
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
    }

    public static class ObjectExtensions
    {
        public static void SetPropertyInfoValue(this object entity, string propertyName, object value)
        {
            entity.SetPropertyInfoValue(entity.GetType().GetProperty(propertyName), value);
        }

        public static void SetPropertyInfoValue(this object entity, PropertyInfo property, object value)
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
    }

    public static class NumberExtensions
    {
        public static bool IsNumeric(this object o)
        {
            var result = 0L;

            return long.TryParse(o.ToString(), out result);
        }
    }

    public static class StringExtension
    {
        public static string ReplaceFirst(this string text, string search, string replace)
        {
            var pos = text.IndexOf(search, StringComparison.Ordinal);

            if (pos < 0)
            {
                return text;
            }

            return text.Substring(0, pos) + replace + text.Substring(pos + search.Length);
        }
    }
}
