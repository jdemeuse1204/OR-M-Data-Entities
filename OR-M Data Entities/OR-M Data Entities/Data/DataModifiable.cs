/*
 * OR-M Data Entities v1.2.0
 * License: The MIT License (MIT)
 * Code: https://github.com/jdemeuse1204/OR-M-Data-Entities
 * Copyright (c) 2015 James Demeuse
 */

using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using OR_M_Data_Entities.Commands;
using OR_M_Data_Entities.Data.Commit;
using OR_M_Data_Entities.Mapping;

namespace OR_M_Data_Entities.Data
{
    /// <summary>
    /// This class uses DataFetching functions to Save, Delete, and Find
    /// </summary>
    public abstract class DataModifiable : DataFetching
    {
        #region Events And Delegates
        public delegate void OnBeforeSaveHandler(DataModifiable context, object entity);

        public event OnBeforeSaveHandler OnBeforeSave;
        #endregion

        #region Constructor
        protected DataModifiable(string connectionStringOrName)
            : base(connectionStringOrName)
        {
        }
        #endregion

        #region Save Methods
        public virtual void SaveChanges<T>(T entity)
            where T : class
        {
            lock (Lock)
            {
                if (DatabaseSchemata.HasForeignKeys(entity))
                {
                    var savableObjects = new List<ForeignKeySaveNode> { new ForeignKeySaveNode(null, entity, null) };

                    // creates the save order based on the primary and foreign keys
                    _analyzeObjectWithForeignKeysAndGetModificationOrder(entity, savableObjects);

                    if (OnBeforeSave != null)
                    {
                        OnBeforeSave(this, entity);
                    }

                    foreach (var savableObject in savableObjects)
                    {
                        var isList = savableObject.Property != null && savableObject.Property.PropertyType.IsList();

                        if (isList)
                        {
                            // relationship is one-many.  Need to set the foreign key before saving
                            _setValue(savableObject.Parent, savableObject.Value);
                        }

                        _saveObjectToDatabase(savableObject.Value);

                        if (!isList)
                        {
                            // relationship is one-one.  Need to set the foreign key after saving
                            _setValue(savableObject.Parent, savableObject.Value);
                        }
                    }

                    return;
                }

                if (OnBeforeSave != null)
                {
                    OnBeforeSave(this, entity);
                }

                _saveObjectToDatabase(entity);
            }
        }

        private void _setValue(object parent, object child)
        {
            if (parent == null) return;

            var foreignKeyProperty =
                DatabaseSchemata.GetForeignKeys(parent)
                    .First(
                        w =>
                            (w.PropertyType.IsList()
                                ? w.PropertyType.GetGenericArguments()[0]
                                : w.PropertyType) == child.GetType());

            var foreignKeyAttribute = foreignKeyProperty.GetCustomAttribute<ForeignKeyAttribute>();

            if (foreignKeyProperty.PropertyType.IsList())
            {
                var parentPrimaryKey = DatabaseSchemata.GetPrimaryKeys(parent).First();
                var value = parent.GetType().GetProperty(parentPrimaryKey.Name).GetValue(parent);

                DatabaseEntity.SetPropertyValue(child, foreignKeyAttribute.ForeignKeyColumnName, value);
            }
            else
            {
                var childPrimaryKey = DatabaseSchemata.GetPrimaryKeys(child).First();
                var value = child.GetType().GetProperty(childPrimaryKey.Name).GetValue(child);

                DatabaseEntity.SetPropertyValue(parent, foreignKeyAttribute.ForeignKeyColumnName, value);
            }
        }

        private void _saveObjectToDatabase<T>(T entity)
        {
            // Check to see if the PK is defined
            var tableName = DatabaseSchemata.GetTableName(entity);

            // ID is the default primary key name
            var primaryKeys = DatabaseSchemata.GetPrimaryKeys(entity);

            // all table properties
            var tableColumns = DatabaseSchemata.GetTableFields(entity);

            // check to see whether we have an insert or update
            var state = DatabaseEntity.GetState(entity, primaryKeys);

            // Update Or Insert data
            switch (state)
            {
                case ModificationState.Update:
                    {
                        // Update Data
                        var update = new SqlUpdateBuilder();
                        update.Table(tableName);

                        foreach (var property in from property in tableColumns let columnName = DatabaseSchemata.GetColumnName(property) where !primaryKeys.Select(w => w.Name).Contains(property.Name) select property)
                        {
                            // Skip unmapped fields
                            update.AddUpdate(property, entity);
                        }

                        // add validation to only update the row
                        foreach (var primaryKey in primaryKeys)
                        {
                            update.AddWhere(tableName, primaryKey.Name, ComparisonType.Equals, primaryKey.GetValue(entity));
                        }

                        Execute(update);
                    }
                    break;
                case ModificationState.Insert:
                    {
                        // Insert Data
                        var insert = new SqlInsertBuilder();

                        insert.Table(tableName);

                        // Loop through all mapped properties
                        foreach (var property in tableColumns)
                        {
                            insert.AddInsert(property, entity);
                        }

                        // Execute the insert statement
                        Execute(insert);

                        // set the resulting pk(s) and db generated columns in the entity object
                        foreach (var item in SelectIdentity())
                        {
                            // find the property first in case the column name change attribute is used
                            // Key is property name, value is the db value
                            DatabaseEntity.SetPropertyValue(
                                entity,
                                item.Key,
                                item.Value);
                        }
                    }
                    break;
            }

            // close our readers
            Reader.Close();
            Reader.Dispose();
        }
        #endregion

        #region Delete Methods
        public virtual bool Delete<T>(T entity)
            where T : class
        {
            lock (Lock)
            {
                if (!DatabaseSchemata.HasForeignKeys(entity)) return _deleteObjectFromDatabase(entity);

                var result = true;
                var savableObjects = new List<ForeignKeySaveNode> { new ForeignKeySaveNode(null, entity, null) };

                // creates the save order based on the primary and foreign keys
                _analyzeObjectWithForeignKeysAndGetModificationOrder(entity, savableObjects);

                // need to reverse the save order for a delete
                savableObjects.Reverse();

                foreach (var savableObject in savableObjects.Where(savableObject => !_deleteObjectFromDatabase(savableObject.Value)))
                {
                    result = false;
                }

                return result;
            }
        }

        private bool _deleteObjectFromDatabase<T>(T entity)
        {
            // Check to see if the PK is defined
            var tableName = DatabaseSchemata.GetTableName(entity);

            // ID is the default primary key name
            var primaryKeys = DatabaseSchemata.GetPrimaryKeys(entity);

            // delete Data
            var builder = new SqlDeleteBuilder();
            builder.Table(tableName);

            // Loop through all mapped properties
            foreach (var property in primaryKeys)
            {
                var value = property.GetValue(entity);
                var columnName = DatabaseSchemata.GetColumnName(property);
                builder.AddWhere(tableName, columnName, ComparisonType.Equals, value);
            }

            // Execute the insert statement
            Execute(builder);

            if (!Reader.HasRows) return false;

            Read();

            var rowsAffected = Reader.GetInt32(0);

            // close our readers
            Reader.Close();
            Reader.Dispose();

            // return if anything was deleted
            return rowsAffected > 0;
        }
        #endregion

        #region Helpers
        /// <summary>
        /// Analyzes the object to be saved to make sure all foreign keys have a object to be saved.  
        /// Save is cancelled if there are errors.  Also creates the save order based on the foreign keys
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="entity"></param>
        /// <param name="savableObjects"></param>
        private void _analyzeObjectWithForeignKeysAndGetModificationOrder<T>(T entity, List<ForeignKeySaveNode> savableObjects)
            where T : class
        {
            // make sure FKs have values before saving, if they dont you need to throw an error
            // we want to look at one-one relationships before one-many
            foreach (var foreignKey in DatabaseSchemata.GetForeignKeys(entity).OrderBy(w => w.PropertyType.IsList()))
            {
                var index = 0;
                var foreignKeyValue = foreignKey.GetValue(entity);
                var foreignKeyIsList = foreignKey.PropertyType.IsList();

                if (foreignKeyValue == null)
                {
                    if (foreignKeyIsList) continue;

                    // list can be one-many or one-none.  We assume the key to the primary table is in this table therefore the base table can still be saved while
                    // maintaining the relationship
                    throw new Exception(string.Format("SAVE CANCELLED!  Reason: Foreign Key Has No Value - Foreign Key Property Name: {0}", foreignKey.Name));
                }

                // doesnt have dependencies
                if (foreignKeyIsList)
                {
                    foreach (var item in (foreignKeyValue as dynamic))
                    {
                        // ForeignKeySaveNode implements IEquatable and Overrides get hash code to only compare
                        // the value property
                        index = savableObjects.IndexOf(new ForeignKeySaveNode(null, entity, null));

                        savableObjects.Insert(index + 1, new ForeignKeySaveNode(foreignKey, item, entity));

                        if (DatabaseSchemata.HasForeignKeys(item))
                        {
                            _analyzeObjectWithForeignKeysAndGetModificationOrder(item, savableObjects);
                        }
                    }
                }
                else
                {
                    // must be saved before the parent
                    // ForeignKeySaveNode implements IEquatable and Overrides get hash code to only compare
                    // the value property
                    index = savableObjects.IndexOf(new ForeignKeySaveNode(null, entity, null));

                    savableObjects.Insert(index, new ForeignKeySaveNode(foreignKey, foreignKeyValue, entity));

                    // has dependencies
                    if (DatabaseSchemata.HasForeignKeys(foreignKeyValue))
                    {
                        _analyzeObjectWithForeignKeysAndGetModificationOrder(foreignKeyValue as dynamic, savableObjects);
                    }
                }
            }
        }
        #endregion
    }
}
