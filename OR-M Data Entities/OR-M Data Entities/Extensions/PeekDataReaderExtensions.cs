/*
 * OR-M Data Entities v3.0
 * License: The MIT License (MIT)
 * Code: https://github.com/jdemeuse1204/OR-M-Data-Entities
 * Email: james.demeuse@gmail.com
 * Copyright (c) 2014 James Demeuse
 */

using System;
using System.Collections.Generic;
using System.Data;
using System.Dynamic;
using System.Linq;
using System.Reflection;
using OR_M_Data_Entities.Data;
using OR_M_Data_Entities.Data.Definition;
using OR_M_Data_Entities.Exceptions;
using OR_M_Data_Entities.Mapping;
using OR_M_Data_Entities.Mapping.Base;

// ReSharper disable once CheckNamespace
namespace OR_M_Data_Entities
{
    public static class PeekDataReaderExtensions
    {
        /// <summary>
        /// Turns a record into a dynamic object
        /// </summary>
        /// <param name="reader"></param>
        /// <returns></returns>
        public static dynamic ToDynamic(this PeekDataReader reader)
        {
            if (!reader.HasRows) return null;

            var result = new ExpandoObject() as IDictionary<string, object>;

            var rec = (IDataRecord)reader;

            for (var i = 0; i < rec.FieldCount; i++) result.Add(rec.GetName(i), rec.GetValue(i));

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
            if (reader.HasRows) return reader._toObject<T>();

            // clean up reader
            reader.Dispose();

            throw new DataException("Query contains no records");
        }

        /// <summary>
        /// Will return default if no rows exist
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="reader"></param>
        /// <returns></returns>
        public static T ToObjectDefault<T>(this PeekDataReader reader)
        {
            if (reader.HasRows) return reader._toObject<T>();

            // clean up reader
            reader.Dispose();

            // return the default
            return default(T);
        }

        private static T _toObject<T>(this PeekDataReader reader)
        {
            //if (typeof(T).IsValueType || typeof(T) == typeof(string)) return reader[0] == DBNull.Value ? default(T) : (T)reader[0];

            //if (typeof(T) == typeof(object)) return reader.ToDynamic();

            //       // if its an anonymous type, use the correct loader
            //return typeof(T).IsAnonymousType() ? (T)reader._getAnonymousObject(typeof(T))

            //       // if the payload is null, load by column names
            //       : reader.Payload == null ? reader._getObjectFromReaderUsingDatabaseColumnNames<T>()

            //       // if the payload has foreign keys, use the foreign key loader
            //       : reader.Payload.Query.HasForeignKeys ? reader._getObjectFromReaderWithForeignKeys<T>()

            //       // default if all are false
            //       : reader._getObjectFromReader<T>();

            return default(T);
        }

        #region Load Object Methods
        private static bool _loadObject(this PeekDataReader reader, object instance, string parentPropertyName)
        {
            try
            {
                //List<DbColumn> properties;

                //// decide how to select columns
                //// They should be ordered by the Primary key.  If the primary key is a dbnull then we do
                //// not want to load the object because the rest is null.  Set the object to null
                //if (parentPropertyName == null)
                //{
                //    // Parent object
                //    properties =
                //        reader.Payload.Query.SelectInfos.Where(
                //            w => w.NewTable.Type == instance.GetType() && w.IsSelected)
                //            .OrderByDescending(w => w.IsPrimaryKey)
                //            .ToList();
                //}
                //else
                //{
                //    // foreign/pseudo keys
                //    properties =
                //        reader.Payload.Query.SelectInfos.Where(
                //            w =>
                //                w.NewTable.Type == instance.GetType() && w.IsSelected &&
                //                w.ParentPropertyName == parentPropertyName).OrderByDescending(w => w.IsPrimaryKey).ToList();
                //}

                //foreach (var property in properties)
                //{
                //    var ordinal = reader.Payload.Query.GetOrdinalBySelectedColumns(property.Ordinal);
                //    var dbValue = reader[ordinal];
                //    var dbNullValue = dbValue as DBNull;

                //    // the rest of the object will be null.  No data exists for the object
                //    if (property.IsPrimaryKey && dbNullValue != null) return false;

                //    instance.SetPropertyInfoValue(property.NewPropertyName, dbNullValue != null ? null : dbValue);
                //}

                return true;
            }
            catch (Exception ex)
            {
                throw new DataLoadException(string.Format("PeekDataReader could not load object {0}.  Message: {1}",
                    instance.GetType().Name, ex.Message));
            }
        }

        private static void _loadObjectByColumnNames(this PeekDataReader reader, object instance)
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
            }
            catch (Exception ex)
            {
                throw new DataLoadException(string.Format("PeekDataReader could not load object {0}.  Message: {1}",
                    instance.GetType().Name, ex.Message));
            }
        }

        private static object _getValue(this PeekDataReader reader, Type instanceType, string propertyName)
        {
            //var table = reader.Payload.Query.Tables.Find(instanceType, reader.Payload.Query.Id);
            //var property = reader.Payload.Query.SelectInfos.First(w => w.Table.Type == table.Type && w.IsSelected && w.NewPropertyName == propertyName);
            //var ordinal = reader.Payload.Query.GetOrdinalBySelectedColumns(property.Ordinal);

            //return reader[ordinal];

            return null;
        }

        private static T _getObjectFromReaderWithForeignKeys<T>(this PeekDataReader reader)
        {
            // Create instance
            var instance = Activator.CreateInstance<T>();

            // get the key so we can look at the key of each row
            //var compositeKeyArray = reader.Payload.Query.LoadSchematic._getCompositKeyArray(reader);

            //// grab the starting composite key
            //var compositeKey = reader.Payload.Query.LoadSchematic._getCompositKey(reader);

            //// load the instance
            //reader._loadObject(instance, null);

            //// set the table on load if possible, we don't care about foreign keys
            //DatabaseSchematic.TrySetPristineEntity(instance);

            //// load first row, do not move next.  While loop will move next 
            //_loadObjectWithForeignKeys(reader, instance);

            //// Loop through the dataset and fill our object.  Check to see if the next PK is the same as the starting PK
            //// if it is then we need to stop and return our object
            //while (reader.Peek() &&
            //    compositeKey.Equals(compositeKeyArray.Sum(w => reader[w].GetHashCode())) &&
            //    reader.Read())
            //{
            //    _loadObjectWithForeignKeys(reader, instance);
            //}

            //// Rows with a PK from the initial object are done loading.  Clear Schematics
            //reader.Payload.Query.LoadSchematic.ClearRowReadCache();

            return instance;
        }

        private static T _getObjectFromReader<T>(this PeekDataReader reader)
        {
            // Create instance
            var instance = Activator.CreateInstance<T>();

            // load the instance
            reader._loadObject(instance, null);

            // set the table on load if possible, we don't care about foreign keys
            DatabaseSchematic.TrySetPristineEntity(instance);

            return instance;
        }

        private static T _getObjectFromReaderUsingDatabaseColumnNames<T>(this PeekDataReader reader)
        {
            // Create instance
            var instance = Activator.CreateInstance<T>();

            // load the instance
            reader._loadObjectByColumnNames(instance);

            // set the table on load if possible
            DatabaseSchematic.TrySetPristineEntity(instance);

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

        private static void _loadObjectWithForeignKeys(PeekDataReader reader, object startingInstance)
        {
            // after this method is completed we need to make sure we can read the next set.  This method should go in a loop
            // load the instance before it comes into thos method

            var schematic = new object();// reader.Payload.Query.LoadSchematic;
            var schematicsToScan = new List<OSchematic>();
            var parentInstance = startingInstance;

            // initialize the list



            //schematicsToScan.AddRange(schematic.Children);

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
                DatabaseSchematic.TrySetPristineEntity(newInstance);

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
                    foreach (var child in currentSchematic.Children) child.ReferenceToCurrent = newInstance;

                    continue;
                }

                var foundInstanceForSingleSetValue = _getInstance(i, originalCount, currentSchematic, parentInstance);

                // Single Instance
                foundInstanceForSingleSetValue.GetType()
                    .GetProperty(currentSchematic.PropertyName)
                    .SetValue(foundInstanceForSingleSetValue, newInstance);

                // store references to the current instance so we can load the objects,
                // otherwise we will have to search through the object and look for the instance
                foreach (var child in currentSchematic.Children) child.ReferenceToCurrent = newInstance;
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
            //var infos = reader.Payload.Query.SelectInfos.Where(
            //    w => w.NewTable.Type == schematic.Type && schematic.PrimaryKeyNames.Contains(w.NewPropertyName));

            //return infos.Select(w => w.Ordinal).ToArray();

            return null;
        }
        #endregion
    }
}
