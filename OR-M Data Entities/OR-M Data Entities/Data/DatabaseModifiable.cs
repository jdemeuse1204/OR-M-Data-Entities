/*
 * OR-M Data Entities v2.3
 * License: The MIT License (MIT)
 * Code: https://github.com/jdemeuse1204/OR-M-Data-Entities
 * Email: james.demeuse@gmail.com
 * Copyright (c) 2014 James Demeuse
 */

using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Linq;
using System.Reflection;
using OR_M_Data_Entities.Commands;
using OR_M_Data_Entities.Data.Definition;
using OR_M_Data_Entities.Data.Execution;
using OR_M_Data_Entities.Enumeration;
using OR_M_Data_Entities.Exceptions;
using OR_M_Data_Entities.Extensions;
using OR_M_Data_Entities.Mapping;
using OR_M_Data_Entities.Mapping.Base;
using OR_M_Data_Entities.Tracking;

namespace OR_M_Data_Entities.Data
{
    /// <summary>
    /// This class uses DataFetching functions to Save, Delete
    /// </summary>
    public abstract class DatabaseModifiable : DatabaseFetching
    {
        #region Events And Delegates
        public delegate void OnBeforeSaveHandler(object entity);

        public event OnBeforeSaveHandler OnBeforeSave;

        public delegate void OnAfterSaveHandler(object entity);

        public event OnAfterSaveHandler OnAfterSave;

        public delegate void OnSavingHandler(object entity);

        public event OnSavingHandler OnSaving;
        #endregion

        #region Constructor
        protected DatabaseModifiable(string connectionStringOrName)
            : base(connectionStringOrName)
        {
        }
        #endregion

        #region Methods
        public virtual UpdateType SaveChanges<T>(T entity)
            where T : class
        {

            var state = UpdateType.Insert;
            var readOnlyAttribute = entity.GetType().GetCustomAttribute<ReadOnlyAttribute>();

            if (readOnlyAttribute != null)
            {
                switch (readOnlyAttribute.ReadOnlySaveOption)
                {
                    // skip children(foreign keys) if option is set
                    case ReadOnlySaveOption.Skip:
                        return UpdateType.Skip;

                    // Check for readonly attribute and see if we should throw an error   
                    case ReadOnlySaveOption.ThrowException:
                        throw new SqlSaveException(string.Format(
                            "Table Is ReadOnly.  Table: {0}.  Change ReadOnlySaveOption to Skip if you wish to skip this table and its foreign keys",
                            entity.GetTableName()));
                }
            }

            if (entity.HasForeignKeys())
            {
                var savableObjects = new List<ForeignKeySaveNode> { new ForeignKeySaveNode(null, entity, null) };

                // creates the save order based on the primary and foreign keys
                _analyzeObjectWithForeignKeysAndGetModificationOrder(entity, savableObjects);

                if (OnBeforeSave != null) OnBeforeSave(entity);

                foreach (var savableObject in savableObjects)
                {
                    var isList = savableObject.Property != null && savableObject.Property.PropertyType.IsList();

                    if (isList)
                    {
                        // relationship is one-many.  Need to set the foreign key before saving
                        _setValue(savableObject.Parent, savableObject.Value, savableObject.Property.Name);
                    }

                    state = _saveObjectToDatabase(savableObject.Value);

                    if (OnAfterSave != null) OnAfterSave(entity);

                    if (savableObject.Parent == null) continue;

                    if (!isList)
                    {
                        // relationship is one-one.  Need to set the foreign key after saving
                        _setValue(savableObject.Parent, savableObject.Value, savableObject.Property.Name);
                    }
                }

                return state;
            }

            if (OnBeforeSave != null) OnBeforeSave(entity);

            state = _saveObjectToDatabase(entity);

            if (OnAfterSave != null) OnAfterSave(entity);

            return state;
        }

        private void _setValue(object parent, object child, string propertyNameToSet)
        {
            if (parent == null) return;

            var foreignKeyProperty =
                parent.GetForeignKeys()
                    .First(
                        w =>
                            (w.PropertyType.IsList()
                                ? w.PropertyType.GetGenericArguments()[0]
                                : w.PropertyType) == child.GetType() &&
                                w.Name == propertyNameToSet);

            var foreignKeyAttribute = foreignKeyProperty.GetCustomAttribute<ForeignKeyAttribute>();

            if (foreignKeyProperty.PropertyType.IsList())
            {
                var parentPrimaryKey = parent.GetPrimaryKeys().First();
                var value = parent.GetType().GetProperty(parentPrimaryKey.Name).GetValue(parent);

                DatabaseEntity.SetPropertyValue(child, foreignKeyAttribute.ForeignKeyColumnName, value);
            }
            else
            {
                var childPrimaryKey = child.GetPrimaryKeys().First();
                var value = child.GetType().GetProperty(childPrimaryKey.Name).GetValue(child);

                DatabaseEntity.SetPropertyValue(parent, foreignKeyAttribute.ForeignKeyColumnName, value);
            }
        }

        private UpdateType _saveObjectToDatabase<T>(T entity)
        {
            // Check to see if the user is using entity state tracking
            var entityTrackable = entity as EntityStateTrackable;

            EntityStateComparePackage entityStatePackage = null;

            // check to see if EntityTrackable is being used, if so check
            // to see if we have any changes
            if (entityTrackable != null)
            {
                entityStatePackage = EntityStateAnalyzer.Analyze(entityTrackable);

                if (entityStatePackage.State == EntityState.UnChanged) return UpdateType.Skip;
            }

            // ID is the default primary key name
            var primaryKeys = entity.GetPrimaryKeys();

            // all table properties
            var tableColumns = entity.GetTableFields();

            // check to see whether we have an insert or update
            var state = DatabaseEntity.GetState(entity, primaryKeys);

            if (OnSaving != null) OnSaving(entity);

            // Update Or Insert data
            switch (state)
            {
                case UpdateType.Update:
                    {
                        // Update Data
                        var update = new SqlUpdateBuilder();

                        update.Table(entity.GetType());

                        var properties = from property in
                                             (from property in tableColumns
                                              let columnName = property.GetColumnName()
                                              where
                                                  !primaryKeys.Select(w => w.Name).Contains(property.Name) &&
                                                  property.GetCustomAttribute<NonSelectableAttribute>() == null // Skip unmapped fields
                                              select property)
                                         let typeAttribute = property.GetCustomAttribute<DbTypeAttribute>()
                                         where typeAttribute == null || typeAttribute.Type != SqlDbType.Timestamp
                                         select property;

                        // are we using a trackable entity? If so only grab the fields to update
                        if (entityTrackable != null)
                        {
                            properties = properties.Where(w => entityStatePackage.ChangeList.Contains(w.Name));
                        }

                        foreach (var property in properties)
                        {
                            update.AddUpdate(property, entity);
                        }

                        // add validation to only update the row
                        foreach (var primaryKey in primaryKeys)
                        {
                            update.AddWhere("", primaryKey.GetColumnName(), CompareType.Equals, primaryKey.GetValue(entity));
                        }

                        // if its only an update, perform the update
                        ExecuteReader(update);

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
                case UpdateType.Insert:
                case UpdateType.TryInsert: // only for tables that have PK's only, no other columns
                case UpdateType.TryInsertUpdate:
                    {
                        // Get The Insert Data
                        var insert = _getInsertBuilder(entity, tableColumns, state);

                        // do not need to read back the values because they are already in the database, if not they will be inserted.
                        // db generation option is always none so there is no need to load any PK's in to the object
                        // Execute the insert statement
                        ExecuteReader(insert);

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

            // Mark the table as unchanged again
            if (entityTrackable != null) EntityStateAnalyzer.TrySetPristineEntity(entity);

            // close our readers
            Connection.Close();
            Reader.Close();
            Reader.Dispose();

            return state;
        }

        private SqlInsertBuilder _getInsertBuilder(object entity, List<PropertyInfo> tableColumns, UpdateType updateType)
        {
            SqlInsertBuilder insert;

            if (updateType == UpdateType.Insert) insert = new SqlInsertBuilder();
            else if (updateType == UpdateType.TryInsert) insert = new SqlTryInsertBuilder();
            else insert = new SqlTryInsertUpdateBuilder();

            insert.Table(entity.GetType());

            // Loop through all mapped properties
            foreach (var property in tableColumns)
            {
                insert.AddInsert(property, entity);
            }

            return insert;
        }

        #endregion

        #region Delete Methods

        //public virtual bool DeleteAll<T>(Expression<Func<T, bool>> expression)
        //{
        //    return false;
        //}

        public virtual bool Delete<T>(T entity)
            where T : class
        {

            var readOnlyAttribute = entity.GetType().GetCustomAttribute<ReadOnlyAttribute>();

            if (readOnlyAttribute != null)
            {
                // skip children(foreign keys) if option is set
                if (readOnlyAttribute.ReadOnlySaveOption == ReadOnlySaveOption.Skip) return false;

                // Check for readonly attribute and see if we should throw an error
                if (readOnlyAttribute.ReadOnlySaveOption == ReadOnlySaveOption.ThrowException)
                {
                    throw new SqlSaveException(string.Format(
                        "Table Is ReadOnly.  Table: {0}.  Change ReadOnlySaveOption to Skip if you wish to skip this table and its foreign keys",
                        entity.GetTableName()));
                }
            }

            if (!entity.HasForeignKeys()) return _deleteObjectFromDatabase(entity);

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

        private bool _deleteObjectFromDatabase<T>(T entity)
        {
            // Check to see if the PK is defined
            var tableName = entity.GetTableName();

            // ID is the default primary key name
            var primaryKeys = entity.GetPrimaryKeys();

            // delete Data
            var builder = new SqlDeleteBuilder();
            builder.Table(tableName);

            // Loop through all mapped properties
            foreach (var property in primaryKeys)
            {
                var value = property.GetValue(entity);
                var columnName = property.GetColumnName();
                builder.AddWhere(tableName, columnName, CompareType.Equals, value);
            }

            try
            {
                // Execute the insert statement
                ExecuteReader(builder);
            }
            catch (SqlException exception)
            {
                // If there is a reference constraint error throw exception noting that the user should try and use a lookup table
                // if they do not want the data to be deleted
                if (!exception.Message.ToUpper().Contains("REFERENCE CONSTRAINT"))
                {
                    throw new SqlReferenceConstraintException(
                        string.Format(
                            "Reference Constraint Violated, consider using a LookupTable to preserve FK Data.  Original Exception: {0}",
                            exception.Message));
                };

                throw;
            }

            if (!Reader.HasRows) return false;

            if (!Reader.IsClosed) Read();

            var rowsAffected = Reader.IsClosed ? 0 : Reader.GetInt32(0);

            // close our readers
            Connection.Close();
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
            foreach (var foreignKey in entity.GetForeignKeys().OrderBy(w => w.PropertyType.IsList()))
            {
                int index;
                var foreignKeyValue = foreignKey.GetValue(entity);
                var foreignKeyIsList = foreignKey.PropertyType.IsList();
                var readOnlyAttribute = foreignKey.GetPropertyType().GetCustomAttribute<ReadOnlyAttribute>();
                var isLookupTable = foreignKey.GetPropertyType().GetCustomAttribute<LookupTableAttribute>() != null;

                // skip lookup tables
                if (isLookupTable) continue;

                // skip children(foreign keys) if option is set
                if (readOnlyAttribute != null && readOnlyAttribute.ReadOnlySaveOption == ReadOnlySaveOption.Skip) continue;

                if (foreignKeyValue == null)
                {
                    if (foreignKeyIsList) continue;

                    var columnName = foreignKey.GetCustomAttribute<ForeignKeyAttribute>().ForeignKeyColumnName;
                    var isNullable = entity.GetType().GetProperty(columnName).PropertyType.IsNullable();

                    // we can skip the foreign key if its nullable and one to one
                    if (isNullable) continue;

                    // list can be one-many or one-none.  We assume the key to the primary table is in this table therefore the base table can still be saved while
                    // maintaining the relationship
                    throw new SqlSaveException(string.Format("Foreign Key Has No Value - Foreign Key Property Name: {0}.  If the ForeignKey is nullable, make the ID nullable in the POCO to save", foreignKey.Name));
                }

                // Check for readonly attribute and see if we should throw an error
                if (readOnlyAttribute != null && readOnlyAttribute.ReadOnlySaveOption == ReadOnlySaveOption.ThrowException)
                {
                    throw new SqlSaveException(string.Format(
                            "Table Is ReadOnly.  Table: {0}.  Change ReadOnlySaveOption to Skip if you wish to skip this table and its foreign keys",
                            entity.GetTableName()));
                }

                // doesnt have dependencies
                if (foreignKeyIsList)
                {
                    foreach (var item in (foreignKeyValue as dynamic))
                    {

                        // make sure there are no saving issues
                        DatabaseEntity.GetState(item);

                        // ForeignKeySaveNode implements IEquatable and Overrides get hash code to only compare
                        // the value property
                        index = savableObjects.IndexOf(new ForeignKeySaveNode(null, entity, null));

                        savableObjects.Insert(index + 1, new ForeignKeySaveNode(foreignKey, item, entity));

                        if (SchemaExtensions.HasForeignKeys(item))
                        {
                            _analyzeObjectWithForeignKeysAndGetModificationOrder(item, savableObjects);
                        }
                    }
                }
                else
                {
                    // make sure there are no saving issues
                    DatabaseEntity.GetState(foreignKeyValue);

                    // must be saved before the parent
                    // ForeignKeySaveNode implements IEquatable and Overrides get hash code to only compare
                    // the value property
                    index = savableObjects.IndexOf(new ForeignKeySaveNode(null, entity, null));

                    savableObjects.Insert(index, new ForeignKeySaveNode(foreignKey, foreignKeyValue, entity));

                    // has dependencies
                    if (foreignKeyValue.HasForeignKeys())
                    {
                        _analyzeObjectWithForeignKeysAndGetModificationOrder(foreignKeyValue as dynamic, savableObjects);
                    }
                }
            }
        }
        #endregion
    }
}
