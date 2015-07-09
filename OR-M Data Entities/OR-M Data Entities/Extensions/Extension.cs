/*
 * OR-M Data Entities v2.1
 * License: The MIT License (MIT)
 * Code: https://github.com/jdemeuse1204/OR-M-Data-Entities
 * Email: james.demeuse@gmail.com
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
using System.Runtime.CompilerServices;
using OR_M_Data_Entities.Data;
using OR_M_Data_Entities.Data.Definition;
using OR_M_Data_Entities.Data.Execution;
using OR_M_Data_Entities.Enumeration;
using OR_M_Data_Entities.Exceptions;
using OR_M_Data_Entities.Expressions;
using OR_M_Data_Entities.Expressions.Query.Columns;
using OR_M_Data_Entities.Expressions.Resolution;
using OR_M_Data_Entities.Mapping;
using OR_M_Data_Entities.Mapping.Base;
using OR_M_Data_Entities.Tracking;

namespace OR_M_Data_Entities
{
    public static class ExpressionQueryExtensions
    {
        #region First
        public static TSource First<TSource>(this ExpressionQuery<TSource> source)
        {
            var resolvable = ((IExpressionQueryResolvable) source);

            TSource result;

            using (var reader = resolvable.DbContext.ExecuteQuery(source))
            {
                result = reader.First();
            }

            resolvable.DbContext.Dispose();

            return result;
        }

        public static TSource First<TSource>(this ExpressionQuery<TSource> source, Expression<Func<TSource, bool>> expression)
        {
            source.Where(expression);

            return First(source);
        }

        public static TSource FirstOrDefault<TSource>(this ExpressionQuery<TSource> source)
        {
            var resolvable = ((IExpressionQueryResolvable)source);

            TSource result;

            using (var reader = resolvable.DbContext.ExecuteQuery(source))
            {
                result = reader.FirstOrDefault();
            }

            resolvable.DbContext.Dispose();

            return result;
        }

        public static TSource FirstOrDefault<TSource>(this ExpressionQuery<TSource> source, Expression<Func<TSource, bool>> expression)
        {
            source.Where(expression);

            // execute sql, and grab data
            return FirstOrDefault(source);
        }
        #endregion

        #region Max
        public static decimal? Max(this ExpressionQuery<decimal?> source)
        {
            return _max(source);
        }

        public static decimal Max(this ExpressionQuery<decimal> source)
        {
            return _max(source);
        }

        public static double? Max(this ExpressionQuery<double?> source)
        {
            return _max(source);
        }

        public static double Max(this ExpressionQuery<double> source)
        {
            return _max(source);
        }

        public static float? Max(this ExpressionQuery<float?> source)
        {
            return _max(source);
        }

        public static float Max(this ExpressionQuery<float> source)
        {
            return _max(source);
        }

        public static int? Max(this ExpressionQuery<int?> source)
        {
            return _max(source);
        }

        public static int Max(this ExpressionQuery<int> source)
        {
            return _max(source);
        }

        public static long? Max(this ExpressionQuery<long?> source)
        {
            return _max(source);
        }

        public static long Max(this ExpressionQuery<long> source)
        {
            return _max(source);
        }
        #endregion

        #region Functions
        public static ExpressionQuery<T> Distinct<T>(this ExpressionQuery<T> source)
        {
            ((ExpressionQueryResolvable<T>)source).ResolveDistinct();

            return source;
        }

        public static ExpressionQuery<T> Take<T>(this ExpressionQuery<T> source, int rows)
        {
            ((ExpressionQueryResolvable<T>)source).ResolveTakeRows(rows);

            return source;
        }
        #endregion

        #region Min
        public static decimal? Min(this ExpressionQuery<decimal?> source)
        {
            return _min(source);
        }

        public static decimal Min(this ExpressionQuery<decimal> source)
        {
            return _min(source);
        }

        public static double? Min(this ExpressionQuery<double?> source)
        {
            return _min(source);
        }

        public static double Min(this ExpressionQuery<double> source)
        {
            return _min(source);
        }

        public static float? Min(this ExpressionQuery<float?> source)
        {
            return _min(source);
        }

        public static float Min(this ExpressionQuery<float> source)
        {
            return _min(source);
        }

        public static int? Min(this ExpressionQuery<int?> source)
        {
            return _min(source);
        }

        public static int Min(this ExpressionQuery<int> source)
        {
            return _min(source);
        }

        public static long? Min(this ExpressionQuery<long?> source)
        {
            return _min(source);
        }

        public static long Min(this ExpressionQuery<long> source)
        {
            return _min(source);
        }
        #endregion

        #region Count
        public static TSource Count<TSource>(this ExpressionQuery<TSource> source)
        {
            ((ExpressionQueryResolvable<TSource>)source).ResolveCount();

            return source.FirstOrDefault();
        }

        public static TSource Count<TSource>(this ExpressionQuery<TSource> source, Expression<Func<TSource, bool>> expression)
        {
            source.Where(expression);

            // execute sql, and grab data
            return Count(source);
        }
        #endregion

        #region To List
        public static List<TSource> ToList<TSource>(this ExpressionQuery<TSource> source)
        {
            var resolvable = ((IExpressionQueryResolvable)source);

            List<TSource> result;

            using (var reader = resolvable.DbContext.ExecuteQuery(source))
            {
                result = reader.ToList();
            }

            resolvable.DbContext.Dispose();

            return result;
        }

        public static List<TSource> ToList<TSource>(this ExpressionQuery<TSource> source, Expression<Func<TSource, bool>> expression)
        {
            source.Where(expression);

            return ToList(source);
        }
        #endregion

        public static OrderedExpressionQuery<TSource> OrderBy<TSource, TKey>(this ExpressionQuery<TSource> source,
            Expression<Func<TSource, TKey>> keySelector)
        {
            if (source.HasForeignKeys) throw new OrderByException("Cannot Order Expression Query that has foreign keys.  Consider returning the results then ordering.");

            return ((ExpressionQueryResolvable<TSource>)source).ResolveOrderBy(keySelector);
        }

        public static OrderedExpressionQuery<TSource> OrderByDescending<TSource, TKey>(this ExpressionQuery<TSource> source,
            Expression<Func<TSource, TKey>> keySelector)
        {
            if (source.HasForeignKeys) throw new OrderByException("Cannot Order Expression Query that has foreign keys.  Consider returning the results then ordering.");

            return ((ExpressionQueryResolvable<TSource>)source).ResolveOrderByDescending(keySelector);
        }

        public static OrderedExpressionQuery<TSource> ThenBy<TSource, TKey>(this OrderedExpressionQuery<TSource> source,
            Expression<Func<TSource, TKey>> keySelector)
        {
            if (source.HasForeignKeys) throw new OrderByException("Cannot Order Expression Query that has foreign keys.  Consider returning the results then ordering.");

            return ((ExpressionQueryResolvable<TSource>)source).ResolveOrderBy(keySelector);
        }

        public static OrderedExpressionQuery<TSource> ThenByDescending<TSource, TKey>(this OrderedExpressionQuery<TSource> source,
            Expression<Func<TSource, TKey>> keySelector)
        {
            if (source.HasForeignKeys) throw new OrderByException("Cannot Order Expression Query that has foreign keys.  Consider returning the results then ordering.");

            return ((ExpressionQueryResolvable<TSource>)source).ResolveOrderByDescending(keySelector);
        }

        public static bool Any<TSource>(this ExpressionQuery<TSource> source, Expression<Func<TSource, bool>> expression)
        {
            source.Where(expression);

            return Any(source);
        }

        public static bool Any<TSource>(this ExpressionQuery<TSource> source)
        {
            // only take one, we only care if it exists or not
            source.Take(1);

            var resolvable = ((IExpressionQueryResolvable)source);

            bool result;

            using (var reader = resolvable.DbContext.ExecuteQuery(source))
            {
                result = reader.HasRows;
            }

            resolvable.DbContext.Dispose();

            return result;
        }

        public static ExpressionQuery<TSource> Where<TSource>(this ExpressionQuery<TSource> source, Expression<Func<TSource, bool>> expression)
        {
            ((ExpressionQueryResolvable<TSource>)source).ResolveWhere(expression);

            return source;
        }

        public static ExpressionQuery<TResult> InnerJoin<TOuter, TInner, TKey, TResult>(this ExpressionQuery<TOuter> outer,
            ExpressionQuery<TInner> inner, Expression<Func<TOuter, TKey>> outerKeySelector, Expression<Func<TInner, TKey>> innerKeySelector,
            Expression<Func<TOuter, TInner, TResult>> resultSelector)
        {
            return ((ExpressionQueryResolvable<TOuter>)outer).ResolveJoin(inner, outerKeySelector,
                innerKeySelector, resultSelector, JoinType.Inner);
        }

        public static ExpressionQuery<TResult> LeftJoin<TOuter, TInner, TKey, TResult>(this ExpressionQuery<TOuter> outer,
            ExpressionQuery<TInner> inner, Expression<Func<TOuter, TKey>> outerKeySelector, Expression<Func<TInner, TKey>> innerKeySelector,
            Expression<Func<TOuter, TInner, TResult>> resultSelector)
        {
            return ((ExpressionQueryResolvable<TOuter>)outer).ResolveJoin(inner, outerKeySelector,
                innerKeySelector, resultSelector, JoinType.Left);
        }

        public static ExpressionQuery<TResult> Select<TSource, TResult>(this ExpressionQuery<TSource> source,
            Expression<Func<TSource, TResult>> selector)
        {
            return ((ExpressionQueryResolvable<TSource>)source).ResolveSelect(selector, source);
        }

        public static bool IsExpressionQuery(this MethodCallExpression expression)
        {
            return expression != null && (expression.Method.ReturnType.IsGenericType &&
                                          expression.Method.ReturnType.GetGenericTypeDefinition()
                                              .IsAssignableFrom(typeof (ExpressionQuery<>)));
        }

        public static bool IsExpressionQuery(this Type type)
        {
            return type.IsGenericType &&
                   type.GetGenericTypeDefinition()
                       .IsAssignableFrom(typeof(ExpressionQuery<>)) || type.IsGenericType &&
                   type.GetGenericTypeDefinition()
                       .IsAssignableFrom(typeof(ExpressionQueryResolvable<>));
        }

        public static bool IsExpressionQuery(this object o)
        {
            return IsExpressionQuery(o.GetType());
        }

        private static T _max<T>(this ExpressionQuery<T> source)
        {
            ((ExpressionQueryResolvable<T>)source).ResoveMax();

            return source.FirstOrDefault();
        }

        private static T _min<T>(this ExpressionQuery<T> source)
        {
            ((ExpressionQueryResolvable<T>)source).ResoveMin();

            return source.FirstOrDefault();
        }
    }

    public static class PeekDataReaderExtensions
    {
        /// <summary>
        /// Turns a record into a dynamic object
        /// </summary>
        /// <param name="reader"></param>
        /// <returns></returns>
        public static dynamic ToDynamic(this PeekDataReader reader)
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

        /// <summary>
        /// Will throw an error if no rows exist
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="reader"></param>
        /// <returns></returns>
        public static T ToObject<T>(this PeekDataReader reader)
        {
            if (!reader.HasRows) throw new DataException("Query contains no records");

            return reader._toObject<T>();
        }

        /// <summary>
        /// Will return default if no rows exist
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="reader"></param>
        /// <returns></returns>
        public static T ToObjectDefault<T>(this PeekDataReader reader)
        {
            return !reader.HasRows ? default(T) : reader._toObject<T>();
        }

        private static T _toObject<T>(this PeekDataReader reader)
        {
            if (typeof(T).IsValueType
                || typeof(T) == typeof(string))
            {
                var data = reader[0];

                return data == DBNull.Value ? default(T) : (T)data;
            }

            if (typeof(T) == typeof(object))
            {
                return reader.ToDynamic();
            }

            try
            {
                return typeof (T).IsAnonymousType()
                    ? (T) reader._getAnonymousObject(typeof (T))
                    : reader.Payload == null
                        ? reader._getObjectFromReaderUsingDatabaseColumnNames<T>()
                        : reader.Payload.Query.HasForeignKeys
                            ? reader._getObjectFromReaderWithForeignKeys<T>()
                            : reader._getObjectFromReader<T>();
            }
            catch (StackOverflowException)
            {
                throw new StackOverflowException("Data Load Error:  Object has too many foreign/pseudo keys, please consider making a view or making your model smaller");
            }
        }

        #region Load Object Methods
        private static bool _loadObject(this PeekDataReader reader, object instance, string parentPropertyName)
        {
            try
            {
                List<DbColumn> properties;

                // decide how to select columns
                // They should be ordered by the Primary key.  If the primary key is a dbnull then we do
                // not want to load the object because the rest is null.  Set the object to null
                if (parentPropertyName == null)
                {
                    // Parent object
                    properties =
                        reader.Payload.Query.SelectInfos.Where(
                            w => w.NewTable.Type == instance.GetType() && w.IsSelected)
                            .OrderByDescending(w => w.IsPrimaryKey)
                            .ToList();
                }
                else
                {
                    // foreign/pseudo keys
                    properties =
                        reader.Payload.Query.SelectInfos.Where(
                            w =>
                                w.NewTable.Type == instance.GetType() && w.IsSelected &&
                                w.ParentPropertyName == parentPropertyName).OrderByDescending(w => w.IsPrimaryKey).ToList();
                }

                foreach (var property in properties)
                {
                    var ordinal = reader.Payload.Query.GetOrdinalBySelectedColumns(property.Ordinal);
                    var dbValue = reader[ordinal];

                    // the rest of the object will be null.  No data exists for the object
                    if (property.IsPrimaryKey && dbValue is DBNull) return false;

                    instance.SetPropertyInfoValue(property.NewPropertyName, dbValue is DBNull ? null : dbValue);
                }

                return true;
            }
            catch (Exception ex)
            {
                throw new DataLoadException(string.Format("PeekDataReader could not load object {0}.  Message: {1}",
                    instance.GetType().Name, ex.Message));
            }
        }

        private static bool _loadObjectByColumnNames(this PeekDataReader reader, object instance)
        {
            try
            {
                var properties =
                    instance.GetType()
                        .GetProperties()
                        .Where(w => w.GetCustomAttribute<NonSelectableAttribute>() == null)
                        .ToList();

                foreach (var property in properties)
                {
                    var columnAttribute = property.GetCustomAttribute<ColumnAttribute>();

                    var dbValue = reader[columnAttribute == null ? property.Name : columnAttribute.Name];

                    instance.SetPropertyInfoValue(property, dbValue is DBNull ? null : dbValue);
                }

                return true;
            }
            catch (Exception ex)
            {
                throw new DataLoadException(string.Format("PeekDataReader could not load object {0}.  Message: {1}",
                    instance.GetType().Name, ex.Message));
            }
        }

        private static object _getValue(this PeekDataReader reader, Type instanceType, string propertyName)
        {
            var table = reader.Payload.Query.Tables.Find(instanceType, reader.Payload.Query.Id);
            var property = reader.Payload.Query.SelectInfos.First(w => w.Table.Type == table.Type && w.IsSelected && w.NewPropertyName == propertyName);
            var ordinal = reader.Payload.Query.GetOrdinalBySelectedColumns(property.Ordinal);

            return reader[ordinal];
        }

        private static T _getObjectFromReaderWithForeignKeys<T>(this PeekDataReader reader)
        {
            // Create instance
            var instance = Activator.CreateInstance<T>();

            // get the key so we can look at the key of each row
            var compositeKeyArray = reader.Payload.Query.LoadSchematic._getCompositKeyArray(reader);

            // grab the starting composite key
            var compositeKey = reader.Payload.Query.LoadSchematic._getCompositKey(reader);

            // load the instance
            reader._loadObject(instance, null);

            // set the table on load if possible, we don't care about foreign keys
            EntityStateAnalyzer.TrySetTableOnLoad(instance);

            // load first row, do not move next.  While loop will move next 
            _loadObjectWithForeignKeys(reader, instance);

            // Loop through the dataset and fill our object.  Check to see if the next PK is the same as the starting PK
            // if it is then we need to stop and return our object
            while (reader.Peek() &&
                compositeKey.Equals(compositeKeyArray.Sum(w => reader[w].GetHashCode())) &&
                reader.Read())
            {
                _loadObjectWithForeignKeys(reader, instance);
            }

            return instance;
        }

        private static T _getObjectFromReader<T>(this PeekDataReader reader)
        {
            // Create instance
            var instance = Activator.CreateInstance<T>();

            // load the instance
            reader._loadObject(instance, null);

            // set the table on load if possible, we don't care about foreign keys
            EntityStateAnalyzer.TrySetTableOnLoad(instance);

            return instance;
        }

        private static T _getObjectFromReaderUsingDatabaseColumnNames<T>(this PeekDataReader reader)
        {
            // Create instance
            var instance = Activator.CreateInstance<T>();

            // load the instance
            reader._loadObjectByColumnNames(instance);

            // set the table on load if possible
            EntityStateAnalyzer.TrySetTableOnLoad(instance);

            return instance;
        }

        private static object _getAnonymousObject(this PeekDataReader reader, Type type)
        {
            var constructorParameters = new Queue<object>();
            var properties = type.GetProperties();

            foreach (var property in properties)
            {
                var propertyType = property.PropertyType;

                if (propertyType.IsSerializable)
                {
                    if (propertyType.IsList())
                    {
                        // load the object
                        constructorParameters.Enqueue(_getValue(reader, type, property.Name));
                        continue;
                    }

                    // the we assume its a value type
                    constructorParameters.Enqueue(_getValue(reader, type, property.Name));
                    continue;
                }

                var propertyInstance = _getAnonymousObject(reader, propertyType);

                constructorParameters.Enqueue(propertyInstance);
            }

            // Do last because the constructor needs the premade properties to go into it
            var instance = Activator.CreateInstance(type, constructorParameters.ToArray());

            return instance;
        }

        public static T _buildAnonymousObject<T>(this PeekDataReader reader)
        {
            var constructor = new Stack<object>();
            var properties = typeof(T).GetProperties();

            foreach (var property in properties)
            {
                if (property.GetType().IsSerializable)
                {

                }
            }

            return default(T);
        }

        private static void _loadObjectWithForeignKeys(
            PeekDataReader reader,
            object startingInstance)
        {
            // after this method is completed we need to make sure we can read the next set.  This method should go in a loop
            // load the instance before it comes into thos method

            var schematic = reader.Payload.Query.LoadSchematic;
            var schematicsToScan = new List<OSchematic>();
            var parentInstance = startingInstance;

            // initialize the list
            schematicsToScan.AddRange(schematic.Children);

            // set the original count so we know wether to look in the parent or reference to parent
            var originalCount = schematicsToScan.Count - 1;

            for (var i = 0; i < schematicsToScan.Count; i++)
            {
                var currentSchematic = schematicsToScan[i];
                var compositeKeyArray = currentSchematic._getCompositKeyArray(reader);
                var compositeKey = _getCompositKey(compositeKeyArray, reader);
                var schematicKey = new OSchematicKey(compositeKey, compositeKeyArray);

                // if ReferenceToCurrent is null then its from the parent and we need to check the composite key.  If its not from the 
                // parent we need to check the Reference to current and see if the property has a value.  If not we need to load
                // the instance.  is null property check should only be for a single instance.  If its a list we need 
                // to fall back to checking the composite key to see if it was loaded.  The property is the list, that 
                // is the incorrect check
                var wasLoaded = currentSchematic.ReferenceToCurrent == null || currentSchematic.ActualType.IsList()
                    ? currentSchematic.LoadedCompositePrimaryKeys.Contains(schematicKey)
                    : currentSchematic.ReferenceToCurrent.GetType()
                        .GetProperty(currentSchematic.PropertyName)
                        .GetValue(currentSchematic.ReferenceToCurrent) != null;

                // add children of current instance so they can be scanned
                schematicsToScan.AddRange(currentSchematic.Children);

                // if it was already loaded, continue to next schematic
                if (wasLoaded) continue;

                // create the instance
                var newInstance = Activator.CreateInstance(currentSchematic.Type);

                // mark the object as loaded
                currentSchematic.LoadedCompositePrimaryKeys.Add(schematicKey);

                // load the data into new instance
                // If load returns false, then its a left join, everything might be null
                if (!reader._loadObject(newInstance, currentSchematic.PropertyName)) continue;

                // set the table on load if possible, we don't care about foreign keys
                EntityStateAnalyzer.TrySetTableOnLoad(newInstance);

                // List
                if (currentSchematic.ActualType.IsList())
                {
                    // check and see if the list was created
                    var foundInstanceForListGetValue = _getInstance(i, originalCount, currentSchematic, parentInstance);

                    var list =
                        foundInstanceForListGetValue
                            .GetType()
                            .GetProperty(currentSchematic.PropertyName)
                            .GetValue(foundInstanceForListGetValue);

                    if (list == null)
                    {
                        var foundInstanceForListSetValue = _getInstance(i, originalCount, currentSchematic, parentInstance);

                        // create new list
                        list = Activator.CreateInstance(currentSchematic.ActualType);

                        // set the new list on the parent
                        foundInstanceForListSetValue.GetType()
                            .GetProperty(currentSchematic.PropertyName)
                            .SetValue(foundInstanceForListSetValue, list);
                    }

                    list.GetType().GetMethod("Add").Invoke(list, new[] { newInstance });

                    // store references to the current instance so we can load the objects,
                    // otherwise we will have to search through the object and look for the instance
                    foreach (var child in currentSchematic.Children)
                    {
                        child.ReferenceToCurrent = newInstance;
                    }

                    continue;
                }

                var foundInstanceForSingleSetValue = _getInstance(i, originalCount, currentSchematic, parentInstance);

                // Single Instance
                foundInstanceForSingleSetValue.GetType()
                    .GetProperty(currentSchematic.PropertyName)
                    .SetValue(foundInstanceForSingleSetValue, newInstance);

                // store references to the current instance so we can load the objects,
                // otherwise we will have to search through the object and look for the instance
                foreach (var child in currentSchematic.Children)
                {
                    child.ReferenceToCurrent = newInstance;
                }
            }
        }

        private static object _getInstance(int index, int originalCount, OSchematic schematic, object parentInstance)
        {
            return index <= originalCount ? parentInstance : schematic.ReferenceToCurrent;
        }
        #endregion

        #region Helpers
        private static int _getCompositKey(this OSchematic schematic, PeekDataReader reader)
        {
            return schematic._getCompositKeyArray(reader).Sum(t => reader[t].GetHashCode());
        }

        private static int _getCompositKey(int[] compositeKeyArray, PeekDataReader reader)
        {
            return compositeKeyArray.Sum(t => reader[t].GetHashCode());
        }

        private static int[] _getCompositKeyArray(this OSchematic schematic, PeekDataReader reader)
        {
            var infos = reader.Payload.Query.SelectInfos.Where(
                w => w.NewTable.Type == schematic.Type && schematic.PrimaryKeyNames.Contains(w.NewPropertyName));

            return infos.Select(w => w.Ordinal).ToArray();
        }
        #endregion
    }

    public static class PropertyInfoExtensions
    {
        public static bool IsPropertyTypeList(this PropertyInfo propertyInfo)
        {
            return propertyInfo.PropertyType.IsList();
        }

        public static Type GetPropertyType(this PropertyInfo propertyInfo)
        {
            return propertyInfo.IsPropertyTypeList()
                ? propertyInfo.PropertyType.GetGenericArguments()[0]
                : propertyInfo.PropertyType;
        }
    }

    public static class TypeExtensions
    {
        public static bool IsDynamic(this Type type)
        {
            return type == typeof(IDynamicMetaObjectProvider);
        }

        public static bool IsAnonymousType(this Type type)
        {

            if (type == null)
            {
                throw new ArgumentNullException("type");
            }

            // HACK: The only way to detect anonymous types right now.
            return Attribute.IsDefined(type, typeof(CompilerGeneratedAttribute), false)
                   && type.IsGenericType && type.Name.Contains("AnonymousType")
                   && (type.Name.StartsWith("<>") || type.Name.StartsWith("VB$"))
                   && (type.Attributes & TypeAttributes.NotPublic) == TypeAttributes.NotPublic;

        }
    }

    public static class SqlCommandExtensions
    {
        public static PeekDataReader ExecuteReaderWithPeeking(this SqlCommand cmd)
        {
            return ExecuteReaderWithPeeking(cmd, null);
        }

        public static PeekDataReader ExecuteReaderWithPeeking(this SqlCommand cmd, ISqlPayload payload)
        {
            return new PeekDataReader(cmd, payload);
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
            var property = entity.GetType().GetProperty(propertyName) ??
                           entity.GetType()
                               .GetProperties()
                               .First(
                                   w =>
                                       w.GetCustomAttribute<ColumnAttribute>() != null &&
                                       w.GetCustomAttribute<ColumnAttribute>().Name == propertyName);

            entity.SetPropertyInfoValue(property, value);
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

    public static class SqlCompareObjectExtensions
    {
        public static bool GreaterThan(this object first, object second)
        {
            return false;
        }

        public static bool GreaterThanOrEqual(this object first, object second)
        {
            return false;
        }

        public static bool LessThan(this object first, object second)
        {
            return false;
        }

        public static bool LessThanOrEqual(this object first, object second)
        {
            return false;
        }
    }
}
