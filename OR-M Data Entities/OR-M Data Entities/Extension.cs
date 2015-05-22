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
using System.Data.SqlTypes;
using System.Dynamic;
using System.Linq;
using System.Reflection;
using System.Xml.Linq;
using OR_M_Data_Entities.Data;
using OR_M_Data_Entities.Data.Commit;
using OR_M_Data_Entities.Expressions.ObjectMapping;
using OR_M_Data_Entities.Expressions.ObjectMapping.Base;
using OR_M_Data_Entities.Mapping;
using OR_M_Data_Entities.Mapping.Base;

namespace OR_M_Data_Entities
{
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

        public static T ToObject<T>(this PeekDataReader reader, string viewId = null)
        {
            if (!reader.HasRows) return default(T);

            if (typeof(T).IsValueType
                || typeof(T) == typeof(string))
            {
                var data = reader[0];

                return data == DBNull.Value ? default(T) : (T)data;
            }

            if (typeof(T) == typeof(object) ||
                (reader.Map != null && reader.Map.DataReturnType == ObjectMapReturnType.Dynamic))
            {
                return reader.ToObject();
            }

            if (reader.Map == null) return reader.GetObjectFromReader<T>();

            switch (reader.Map.DataReturnType)
            {
                case ObjectMapReturnType.Basic:
                    return reader.GetObjectFromReaderUsingTableName<T>();
                case ObjectMapReturnType.ForeignKeys:
                    return reader.GetObjectFromReaderWithForeignKeys<T>(viewId);
                case ObjectMapReturnType.MemberInit:
                    return reader.GetObjectFromReaderObjectOrDefault<T>();
                case ObjectMapReturnType.Value:
                    return (T)reader[0];
                default:
                    return default(T);
            }
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

        private static T GetObjectFromReaderObjectOrDefault<T>(this PeekDataReader reader)
        {
            // Create instance
            var instance = Activator.CreateInstance<T>();

            var rec = (IDataRecord)reader;

            for (var i = 0; i < rec.FieldCount; i++)
            {
                var name = rec.GetName(i);
                var value = rec.GetValue(i);

                var table = reader.Map.Tables.First(w => w.Type == typeof(T));
                var column = table.Columns.FirstOrDefault(w => string.Format("{0}{1}", table.TableName, w.Name) == name);
                var propertyName = column.Name;

                instance.SetPropertyInfoValue(propertyName, value is DBNull ? null : value);
            }

            return instance;
        }

        private static bool LoadObjectWithForeignKeys(this PeekDataReader reader, object instance, string tableName)
        {
            try
            {
                // find any unmapped attributes
                var properties = instance.GetType().GetProperties().Where(w => w.GetCustomAttribute<NonSelectableAttribute>() == null).ToList();

                for (var i = 0; i < properties.Count; i++)
                {
                    var property = properties[i];
                    var columnAttribute = property.GetCustomAttribute<ColumnAttribute>();

                    // need to select by tablename and columnname because of joins.  Column names cannot be ambiguous
                    var dbValue = reader[tableName + (columnAttribute != null ? columnAttribute.Name : property.Name)];


                    if (i == 0 && dbValue is DBNull) return false;

                    instance.SetPropertyInfoValue(property, dbValue is DBNull ? null : dbValue);
                }

                return true;
            }
            catch
            {
                return false;
            }
        }

        private static T GetObjectFromReaderWithForeignKeys<T>(this PeekDataReader reader, string viewId = null)
        {
            // Create instance
            var instance = Activator.CreateInstance<T>();
            var tableName = DatabaseSchemata.GetTableName<T>();
            var primaryKey = DatabaseSchemata.GetPrimaryKeys<T>().First();
            var foreignKeys = DatabaseSchemata.GetForeignKeyTypes(instance);
            var primaryKeyLookUpName = string.Format("{0}{1}", tableName, DatabaseSchemata.GetColumnName(primaryKey));

            // load the instance
            reader.LoadObjectWithForeignKeys(instance, tableName);

            var pkValue = instance.GetType().GetProperty(primaryKey.Name).GetValue(instance);

            _loadObjectWithForeignKeysTest(reader, instance, foreignKeys, primaryKeyLookUpName, pkValue, viewId);

            return instance;
        }

        private static void _loadObjectWithForeignKeysTest(
            PeekDataReader reader,
            object instance,
            List<TableInfo> foreignKey,
            string primaryKeyLookUpName,
            object pkValue,
            string viewId = null)
        {
            var currentInstance = instance;

            // skip the first row because its our base which is already loaded
            for (var i = 1; i < foreignKey.Count; i++)
            {
                var row = foreignKey[i];

                if (!string.IsNullOrWhiteSpace(viewId))
                {
                    var view = row.Type.GetCustomAttribute<ViewAttribute>();

                    if (view == null || !view.ViewIds.Contains(viewId))
                    {
                        // check to see how we continue
                        if (_canContinue(ref i, foreignKey.Count, reader, pkValue, primaryKeyLookUpName))
                        {
                            continue;
                        }
                        break;
                    }
                }

                var currentCompositeKey = row.PrimaryKeys.Sum(t => reader[t].GetHashCode());
                var canAddObject = !row.KeyHashesLoaded.Contains(currentCompositeKey);

                // load FK

                if (!canAddObject)
                {
                    // check to see how we continue
                    if (_canContinue(ref i, foreignKey.Count, reader, pkValue, primaryKeyLookUpName))
                    {
                        continue;
                    }
                    break;
                }

                if (row.ParentType != currentInstance.GetType())
                {
                    // if the ParentProperty is null the property is coming from the base
                    var lookupProperty = row.ParentProperty ?? row.Property;

                    // need to search from top to bottom for a type and property name match
                    var currentProperty = _findProperty(row, instance, out currentInstance);

                    if (currentProperty == null)
                    {
                        // we have a missing parent for the child, skip the child

                        // check to see how we continue
                        if (_canContinue(ref i, foreignKey.Count, reader, pkValue, primaryKeyLookUpName))
                        {
                            continue;
                        }
                        break;
                    }

                    // if the parent property is null then do not reset the current instance.  Current instance must be the base object
                    if (row.ParentProperty != null)
                    {
                        currentInstance =
                            currentInstance.GetType().GetProperty(lookupProperty.Name).GetValue(currentInstance);

                        if (currentInstance.IsList())
                        {
                            // get the last item in the list
                            var count = (int)typeof(Enumerable).GetMethods()
                                .First(w => w.Name == "Count")
                                .MakeGenericMethod(lookupProperty.PropertyType.GetGenericArguments()[0])
                                .Invoke(currentInstance, new[] { currentInstance });

                            if (count == 0)
                            {
                                // dont load anything in the list because the previous instance doesnt exist

                                // check to see how we continue
                                if (_canContinue(ref i, foreignKey.Count, reader, pkValue, primaryKeyLookUpName))
                                {
                                    continue;
                                }
                                break;
                            }

                            currentInstance =
                                typeof(Enumerable).GetMethods()
                                    .First(w => w.Name == "Last")
                                    .MakeGenericMethod(lookupProperty.PropertyType.GetGenericArguments()[0])
                                    .Invoke(currentInstance, new[] { currentInstance });

                        }
                    }
                }

                if (row.IsList)
                {
                    // does list exist?
                    var property = currentInstance.GetType().GetProperty(row.Property.Name).GetValue(currentInstance);

                    if (property == null)
                    {
                        // create out list
                        currentInstance.SetPropertyInfoValue(row.Property.Name, Activator.CreateInstance(row.Property.PropertyType));

                        property = currentInstance.GetType().GetProperty(row.Property.Name).GetValue(currentInstance);
                    }

                    // add object
                    // mark the object as loaded 
                    row.KeyHashesLoaded.Add(currentCompositeKey);

                    // grab the instance
                    var childInstance = Activator.CreateInstance(row.Type);

                    if (reader.LoadObjectWithForeignKeys(childInstance, row.TableAlias))
                    {
                        property.GetType().GetMethod("Add").Invoke(property, new[] { childInstance });
                    }
                }
                else
                {
                    // add object
                    // mark the object as loaded 
                    row.KeyHashesLoaded.Add(currentCompositeKey);

                    // grab the instance
                    var childInstance = Activator.CreateInstance(row.Type);

                    if (reader.LoadObjectWithForeignKeys(childInstance, row.TableAlias))
                    {
                        // set on the primary object
                        currentInstance.SetPropertyInfoValue(row.Property.Name, childInstance);
                    }
                }

                // move next
                if (i != (foreignKey.Count - 1)) continue;

                // can we read next?
                if (!reader.Peek()) break;

                // if one column doesnt match then we do not have a match
                if (!pkValue.Equals(reader[primaryKeyLookUpName]))
                {
                    break;
                }

                // read the next row
                reader.Read();
                i = 1;
            }
        }

        private static bool _canContinue(ref int i, int count, PeekDataReader reader, object pkValue, string primaryKeyLookUpName)
        {
            if (i != (count - 1)) return true;

            // can we read next?
            if (!reader.Peek()) return false; // break

            // if one column doesnt match then we do not have a match
            if (!pkValue.Equals(reader[primaryKeyLookUpName])) return false; // break

            // read the next row
            reader.Read();
            i = 0;  // will add one right away because of the continue

            return true;
        }

        private static PropertyInfo _findProperty(TableInfo row, object instance, out object currentInstance)
        {
            currentInstance = instance;

            if (row.ParentProperty == null)
            {
                var parentProperty = instance.GetType()
                    .GetProperties()
                    .FirstOrDefault(
                        w =>
                            w.GetCustomAttribute<ForeignKeyAttribute>() != null && w.Name == row.Property.Name &&
                            w.PropertyType == row.Property.PropertyType);

                if (parentProperty == null) throw new Exception("Cannot find property to set on model");
                return parentProperty;
            }

            var objectsList = new List<object> { instance };

            for (var i = 0; i < objectsList.Count; i++)
            {
                var o = objectsList[i];
                var property = o.GetType()
                    .GetProperties()
                    .FirstOrDefault(
                        w =>
                            w.GetCustomAttribute<ForeignKeyAttribute>() != null && w.Name == row.ParentProperty.Name &&
                            w.PropertyType == row.ParentProperty.PropertyType);

                if (property != null)
                {
                    currentInstance = o;
                    return property;
                }

                if (o.GetType().IsList())
                {
                    // add the last object from the list if it exists to the object list
                    var count = (int)typeof(Enumerable).GetMethods()
                            .First(w => w.Name == "Count")
                            .MakeGenericMethod(o.GetType().GetGenericArguments()[0])
                            .Invoke(o, new[] { o });

                    if (count != 0)
                    {
                        objectsList.Add(typeof(Enumerable).GetMethods()
                            .First(w => w.Name == "Last")
                            .MakeGenericMethod(o.GetType().GetGenericArguments()[0])
                            .Invoke(o, new[] { o }));
                    }
                }

                objectsList.AddRange(o.GetType().GetProperties().Where(w => w.GetCustomAttribute<ForeignKeyAttribute>() != null).Select(source => source.GetValue(o)).Where(item => item != null));
            }

            return null;
        }

        private static void _recursiveLoadWithForeignKeys(
            PeekDataReader reader,
            object childInstance,
            IEnumerable<ForeignKeyDetail> foreignKeys,
            List<ObjectMapNode> objectMapNodes,
            int lastCompositeKey,
            ObjectMap map)
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

                        _recursiveLoadWithForeignKeys(reader, listItem, foreignKey.ChildTypes, node.Children, currentCompositeKey, map);
                        propertyInstance.GetType().GetMethod("Add").Invoke(propertyInstance, new[] { listItem });
                    }
                    else
                    {
                        if (canAddObject)
                        {
                            if (!reader.LoadObjectWithForeignKeys(listItem, foreignKey.PropertyName)) continue; // can only happen on a list because its one to many

                            _recursiveLoadWithForeignKeys(reader, listItem, foreignKey.ChildTypes, node.Children, currentCompositeKey, map);
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

                                _recursiveLoadWithForeignKeys(reader, listItem, foreignKey.ChildTypes, node.Children, currentCompositeKey, map);
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

                    _recursiveLoadWithForeignKeys(reader, propertyInstance, foreignKey.ChildTypes, node.Children, currentCompositeKey, map);
                }
                else
                {
                    _recursiveLoadWithForeignKeys(reader, propertyInstance, foreignKey.ChildTypes, node.Children, currentCompositeKey, map);
                }
            }
        }

        private static void _loadObjectWithForeignKeys(
            PeekDataReader reader,
            object instance,
            IEnumerable<ForeignKeyDetail> foreignKeys,
            string primaryKeyLookUpName,
            object pkValue,
            List<ObjectMapNode> objectMapNodes,
            ObjectMap map)
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

                    // cannot use contains, have to track the last keys selected compared to
                    // the current key(s) selected.  Store them in the mapper List<ForeignKeyDetail>
                    if (
                        !(bool)
                            singleInstance.GetType()
                                .GetMethod("Contains")
                                .Invoke(singleInstance, new[] { childInstance }))
                    {
                        // go down each FK tree and create the child instance
                        _recursiveLoadWithForeignKeys(reader, childInstance, foreignKey.ChildTypes, node.Children, currentCompositeKey, map);

                        singleInstance.GetType().GetMethod("Add").Invoke(singleInstance, new[] { childInstance });

                        if (wasListInstanceCreated)
                        {
                            instance.GetType().GetProperty(foreignKey.PropertyName).SetValue(instance, singleInstance);
                        }
                    }
                    continue;
                }

                _recursiveLoadWithForeignKeys(reader, singleInstance ?? childInstance, foreignKey.ChildTypes, node.Children, currentCompositeKey, map);

                if (singleInstance == null)
                {
                    instance.GetType().GetProperty(foreignKey.PropertyName).SetValue(instance, childInstance);
                }
            }

            // make sure we can peek
            if (!reader.Peek()) return;

            // if one column doesnt match then we do not have a match
            if (!pkValue.Equals(reader[primaryKeyLookUpName]))
            {
                return;
            }

            // read the next row
            reader.Read();

            _loadObjectWithForeignKeys(reader, instance, foreignKeys, primaryKeyLookUpName, pkValue, objectMapNodes, map);
        }
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

        public static PeekDataReader ExecuteReaderWithPeeking(this SqlCommand cmd, ObjectMap map)
        {
            return new PeekDataReader(cmd, map);
        }
    }

    public static class DictionaryExtensions
    {
        public static string GetNextParameter(this Dictionary<string, object> parameters)
        {
            return string.Format("@Param{0}", parameters.Count);
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
