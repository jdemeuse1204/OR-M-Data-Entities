/*
 * OR-M Data Entities v3.0
 * License: The MIT License (MIT)
 * Code: https://github.com/jdemeuse1204/OR-M-Data-Entities
 * Email: james.demeuse@gmail.com
 * Copyright (c) 2014 James Demeuse
 */

using System;
using System.Collections;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Linq;
using System.Reflection;
using OR_M_Data_Entities.Data.Definition.Base;
using OR_M_Data_Entities.Data.Execution;
using OR_M_Data_Entities.Data.Query.StatementParts;
using OR_M_Data_Entities.Enumeration;
using OR_M_Data_Entities.Exceptions;
using OR_M_Data_Entities.Extensions;
using OR_M_Data_Entities.Mapping;
using OR_M_Data_Entities.Mapping.Base;
using OR_M_Data_Entities.Tracking;
using SqlTransaction = OR_M_Data_Entities.Data.Query.StatementParts.SqlTransaction;

namespace OR_M_Data_Entities.Data
{
    /// <summary>
    /// This class uses DataFetching functions to Save, Delete
    /// </summary>
    public abstract partial class DatabaseModifiable : DatabaseFetching
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

        #region Properties

        private readonly string _transactionSqlBase = @"
DECLARE @1 VARCHAR(50) = CONVERT(varchar,GETDATE(),126);

BEGIN TRANSACTION @1;
	BEGIN TRY
			{0}
	END TRY
	BEGIN CATCH

		IF @@TRANCOUNT > 0
			BEGIN
				ROLLBACK TRANSACTION;
			END
			
			DECLARE @2 as varchar(max) = ERROR_MESSAGE() + '  Rollback performed, no data committed.',
					@3 as int = ERROR_SEVERITY(),
					@4 as int = ERROR_STATE();

			RAISERROR(@2,@3,@4);
			
	END CATCH

	IF @@TRANCOUNT > 0
		COMMIT TRANSACTION @1;
";

        #endregion

        #region Methods

        private string _createTransaction(string sql)
        {
            return string.Format(_transactionSqlBase, sql);
        }



        private SqlInsertBuilder _getInsertBuilder(object entity, List<PropertyInfo> tableColumns, UpdateType updateType)
        {
            SqlInsertBuilder insert;

            // get the correct insert builder
            switch (updateType)
            {
                case UpdateType.Insert:
                    insert = new SqlInsertBuilder(Configuration);
                    break;
                case UpdateType.TryInsert:
                    insert = new SqlTryInsertBuilder(Configuration);
                    break;
                case UpdateType.TryInsertUpdate:
                    insert = new SqlTryInsertUpdateBuilder(Configuration);
                    break;
                default:
                    throw new SqlSaveException(string.Format("Cannot use Update Type of {0} with insert builder", updateType));
            }

            insert.Table(entity.GetType());

            // Loop through all mapped properties
            foreach (var property in tableColumns)
            {
                insert.AddInsert(property, entity);
            }

            return insert;
        }

        #endregion

        #region Save Methods

        public virtual UpdateType SaveChanges<T>(T entity)
            where T : class
        {
            // can only use transactions when MARS is on because the execution is different.
            return Configuration.UseMultipleActiveResultSets
                ? _saveChangesUsingTransactions(entity)
                : _saveChanges(entity);
        }

        private UpdateType _saveChanges<T>(T entity)
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
                var savableObjects = new List<ForeignKeySaveNode>();

                // begin transaction

                // creates the save order based on the primary and foreign keys
                _getSaveOrder(entity, savableObjects, Configuration.UseMultipleActiveResultSets);

                if (OnBeforeSave != null) OnBeforeSave(entity);

                foreach (var savableObject in savableObjects)
                {
                    var isList = savableObject.Property != null && savableObject.Property.PropertyType.IsList();

                    if (isList)
                    {
                        // relationship is one-many.  Need to set the foreign key before saving
                        _setPropertyValue(savableObject.Parent, savableObject.Value, savableObject.Property.Name);
                    }

                    state = _saveObjectToDatabase(savableObject.Value);

                    if (OnAfterSave != null) OnAfterSave(entity);

                    if (savableObject.Parent == null) continue;

                    if (!isList)
                    {
                        // relationship is one-one.  Need to set the foreign key after saving
                        _setPropertyValue(savableObject.Parent, savableObject.Value, savableObject.Property.Name);
                    }
                }

                return state;
            }

            if (OnBeforeSave != null) OnBeforeSave(entity);

            state = _saveObjectToDatabase(entity);

            if (OnAfterSave != null) OnAfterSave(entity);

            return state;
        }

        private UpdateType _saveChangesUsingTransactions<T>(T entity)
            where T : class
        {
            var state = UpdateType.Insert;

            // analyze the entity and get the save order.

            // check the save objects to make sure there are no readonly tables

            // create the transactions

            // load the resulting data back into the objects, use 'TableName.ColumnName' to load back into objects,
            // we can do this because the resulting data is not in one giant data set, its in separate ones so no 
            // danger of column names matching

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
                var savableObjects = new List<ForeignKeySaveNode>();
                var transaction = new SqlTransaction();

                // creates the save order based on the primary and foreign keys
                _getSaveOrder(entity, savableObjects, Configuration.UseMultipleActiveResultSets);
                
                foreach (var savableObject in savableObjects)
                {
                    var statement = _getSaveStatement(savableObject.Value);

                    if (statement == null) continue;// skip because nothing changed

                    transaction.Add(statement);
                }

                // execute the transaction

                // if no errors on save make sure to set each entity as pristine

                return state;
            }

            // fire the before save action if there is one
            if (OnBeforeSave != null) OnBeforeSave(entity);

            state = _saveObjectToDatabase(entity);

            // fire the after save action if there is one
            if (OnAfterSave != null) OnAfterSave(entity);

            return state;
        }

        private SqlStatement _getSaveStatement<T>(T entity)
        {
            // Check to see if the user is using entity state tracking
            var entityTrackable = entity as EntityStateTrackable;

            EntityStateComparePackage entityStatePackage = null;

            // check to see if EntityTrackable is being used, if so check
            // to see if we have any changes
            if (entityTrackable != null)
            {
                entityStatePackage = EntityStateAnalyzer.Analyze(entityTrackable);

                if (entityStatePackage.State == EntityState.UnChanged) return null;
            }

            // ID is the default primary key name
            var primaryKeys = entity.GetPrimaryKeys();

            // all table properties
            var tableColumns = entity.GetTableFields();

            // check to see whether we have an insert or update
            var state = _getState(entity, primaryKeys);

            if (OnSaving != null) OnSaving(entity);

            // Update Or Insert data
            switch (state)
            {
                case UpdateType.Update:
                    {
                        // Update Data
                        var update = new SqlUpdateBuilder(Configuration);

                        update.Table(entity.GetType());

                        var properties = from property in
                            (from property in tableColumns
                             let columnName = property.GetColumnName()
                             where
                             !primaryKeys.Select(w => w.Name).Contains(property.Name) &&
                             property.GetCustomAttribute<NonSelectableAttribute>() == null
                             // Skip unmapped fields
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
                            update.AddWhere(primaryKey.GetColumnName(), CompareType.Equals, primaryKey.GetValue(entity));
                        }

                        // get sql string
                        return update;
                    }
                case UpdateType.Insert:
                case UpdateType.TryInsert: // only for tables that have PK's only, no other columns
                case UpdateType.TryInsertUpdate:
                    {
                        // Get The Insert Data
                        return _getInsertBuilder(entity, tableColumns, state);
                    }
            }

            return null;
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
            var state = _getState(entity, primaryKeys);

            if (OnSaving != null) OnSaving(entity);

            // Update Or Insert data
            switch (state)
            {
                case UpdateType.Update:
                    {
                        // Update Data
                        var update = new SqlUpdateBuilder(Configuration);

                        update.Table(entity.GetType());

                        var properties = from property in
                            (from property in tableColumns
                             let columnName = property.GetColumnName()
                             where
                             !primaryKeys.Select(w => w.Name).Contains(property.Name) &&
                             property.GetCustomAttribute<NonSelectableAttribute>() == null
                         // Skip unmapped fields
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
                            update.AddWhere(primaryKey.GetColumnName(), CompareType.Equals, primaryKey.GetValue(entity));
                        }

                        // if its only an update, perform the update
                        ExecuteReader(update);

                        // set the resulting pk(s) and db generated columns in the entity object
                        foreach (var item in SelectIdentity())
                        {
                            // find the property first in case the column name change attribute is used
                            // Key is property name, value is the db value
                            _setPropertyValue(
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
                            _setPropertyValue(
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

        #endregion

        #region Delete Methods

        public virtual bool Delete<T>(T entity)
            where T : class
        {
            return Configuration.UseMultipleActiveResultSets ? _deleteUsingTransactions(entity) : _delete(entity);
        }

        private bool _delete<T>(T entity)
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
            _analyzeObjectWithForeignKeysAndGetModificationOrder(entity, savableObjects, false);

            // need to reverse the save order for a delete
            savableObjects.Reverse();

            foreach (
                var savableObject in
                    savableObjects.Where(savableObject => !_deleteObjectFromDatabase(savableObject.Value)))
            {
                result = false;
            }

            return result;
        }

        private bool _deleteUsingTransactions<T>(T entity)
            where T : class
        {


            return true;
        }

        private bool _deleteObjectFromDatabase<T>(T entity)
        {
            // ID is the default primary key name
            var primaryKeys = entity.GetPrimaryKeys();

            // delete Data
            var builder = new SqlDeleteBuilder(Configuration);
            builder.Table(entity.GetType());

            // Loop through all mapped properties
            foreach (var property in primaryKeys)
            {
                var value = property.GetValue(entity);
                var columnName = property.GetColumnName();
                builder.AddWhere(columnName, CompareType.Equals, value);
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
                }

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
        private void _analyzeObjectWithForeignKeysAndGetModificationOrder<T>(T entity,
            List<ForeignKeySaveNode> savableObjects, bool isMARSEnabled)
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
                if (readOnlyAttribute != null && readOnlyAttribute.ReadOnlySaveOption == ReadOnlySaveOption.Skip)
                    continue;

                if (foreignKeyValue == null)
                {
                    if (foreignKeyIsList) continue;

                    var columnName = foreignKey.GetCustomAttribute<ForeignKeyAttribute>().ForeignKeyColumnName;
                    var isNullable = entity.GetType().GetProperty(columnName).PropertyType.IsNullable();

                    // we can skip the foreign key if its nullable and one to one
                    if (isNullable) continue;

                    if (!isMARSEnabled)
                    {
                        // database will take care of this if MARS is enabled
                        // list can be one-many or one-none.  We assume the key to the primary table is in this table therefore the base table can still be saved while
                        // maintaining the relationship
                        throw new SqlSaveException(
                            string.Format(
                                "Foreign Key Has No Value - Foreign Key Property Name: {0}.  If the ForeignKey is nullable, make the ID nullable in the POCO to save",
                                foreignKey.Name));
                    }
                }

                // Check for readonly attribute and see if we should throw an error
                if (readOnlyAttribute != null &&
                    readOnlyAttribute.ReadOnlySaveOption == ReadOnlySaveOption.ThrowException)
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
                        _getState(item);

                        // ForeignKeySaveNode implements IEquatable and Overrides get hash code to only compare
                        // the value property
                        index = savableObjects.IndexOf(new ForeignKeySaveNode(null, entity, null));

                        savableObjects.Insert(index + 1, new ForeignKeySaveNode(foreignKey, item, entity));

                        if (SchemaExtensions.HasForeignKeys(item))
                        {
                            _analyzeObjectWithForeignKeysAndGetModificationOrder(item, savableObjects, isMARSEnabled);
                        }
                    }
                }
                else
                {
                    // make sure there are no saving issues
                    _getState(foreignKeyValue);

                    // must be saved before the parent
                    // ForeignKeySaveNode implements IEquatable and Overrides get hash code to only compare
                    // the value property
                    index = savableObjects.IndexOf(new ForeignKeySaveNode(null, entity, null));

                    savableObjects.Insert(index, new ForeignKeySaveNode(foreignKey, foreignKeyValue, entity));

                    // has dependencies
                    if (foreignKeyValue.HasForeignKeys())
                    {
                        _analyzeObjectWithForeignKeysAndGetModificationOrder(foreignKeyValue as dynamic, savableObjects,
                            isMARSEnabled);
                    }
                }
            }
        }

        #endregion
    }

    /// <summary>
    /// separate the entity loading code from the modifiable code.  This is the only class that uses the entity loading code
    /// </summary>
    public partial class DatabaseModifiable
    {
        private UpdateType _getState(object entity)
        {
            return _getState(entity, entity.GetPrimaryKeys());
        }

        private UpdateType _getState(object entity, List<PropertyInfo> primaryKeys)
        {
            var areAnyPkGenerationOptionsNone = false;

            var columns =
                entity.GetType().GetProperties().Where(w => !w.IsPrimaryKey()).ToList();

            var entityTrackable = entity as EntityStateTrackable;

            // make sure the user is not trying to update an IDENTITY column, these cannot be updated
            foreach (
                var column in
                    columns.Where(
                        w =>
                            w.GetCustomAttribute<DbGenerationOptionAttribute>() != null &&
                            w.GetCustomAttribute<DbGenerationOptionAttribute>().Option ==
                            DbGenerationOption.IdentitySpecification)
                        .Where(
                            column =>
                                entityTrackable != null &&
                                EntityStateAnalyzer.HasColumnChanged(entityTrackable, column.Name)))
            {
                throw new SqlSaveException(string.Format("Cannot update value if IDENTITY column.  Column: {0}",
                    column.Name));
            }

            for (var i = 0; i < primaryKeys.Count; i++)
            {
                var key = primaryKeys[i];
                var pkValue = key.GetValue(entity);
                var generationOption = key.GetGenerationOption();
                var isUpdating = false;
                var pkValueTypeString = "";
                var pkValueType = "";

                if (generationOption == DbGenerationOption.None) areAnyPkGenerationOptionsNone = true;

                if (generationOption == DbGenerationOption.DbDefault)
                {
                    throw new SqlSaveException("Cannot use DbGenerationOption of DbDefault on a primary key");
                }

                switch (pkValue.GetType().Name.ToUpper())
                {
                    case "INT16":
                        isUpdating = Convert.ToInt16(pkValue) != 0;
                        pkValueTypeString = "zero";
                        pkValueType = "INT16";
                        break;
                    case "INT32":
                        isUpdating = Convert.ToInt32(pkValue) != 0;
                        pkValueTypeString = "zero";
                        pkValueType = "INT32";
                        break;
                    case "INT64":
                        isUpdating = Convert.ToInt64(pkValue) != 0;
                        pkValueTypeString = "zero";
                        pkValueType = "INT64";
                        break;
                    case "GUID":
                        isUpdating = (Guid)pkValue != Guid.Empty;
                        pkValueTypeString = "zero";
                        pkValueType = "INT16";
                        break;
                    case "STRING":
                        isUpdating = !string.IsNullOrWhiteSpace(pkValue.ToString());
                        pkValueTypeString = "null/blank";
                        pkValueType = "STRING";
                        break;
                }

                // break because we are already updating, do not want to set to false
                if (!isUpdating)
                {
                    if (generationOption == DbGenerationOption.None)
                    {
                        // if the db generation option is none and there is no pk value this is an error because the db doesnt generate the pk
                        throw new SqlSaveException(string.Format(
                            "Primary Key cannot be {1} for {2} when DbGenerationOption is set to None.  Primary Key Name: {0}", key.Name,
                            pkValueTypeString, pkValueType));
                    }
                    continue;
                }

                // If we have only primary keys we need to perform a try insert and see if we can try to insert our data.
                // if we have any Pk's with a DbGenerationOption of None we need to first see if a record exists for the pks, 
                // if so we need to perform an update, otherwise we perform an insert
                return entity.HasPrimaryKeysOnly()
                    ? UpdateType.TryInsert
                    : areAnyPkGenerationOptionsNone ? UpdateType.TryInsertUpdate : UpdateType.Update;
            }

            return UpdateType.Insert;
        }

        private void _setPropertyValue(object entity, string propertyName, object value)
        {
            var found = entity.GetType().GetProperty(propertyName);

            if (found == null)
            {
                return;
            }

            var propertyType = found.PropertyType;

            //Nullable properties have to be treated differently, since we 
            //  use their underlying property to set the value in the object
            if (propertyType.IsGenericType
                && propertyType.GetGenericTypeDefinition() == typeof(Nullable<>))
            {
                //if it's null, just set the value from the reserved word null, and return
                if (value == null)
                {
                    found.SetValue(entity, null, null);
                    return;
                }

                //Get the underlying type property instead of the nullable generic
                propertyType = new System.ComponentModel.NullableConverter(found.PropertyType).UnderlyingType;
            }

            //use the converter to get the correct value
            found.SetValue(
                entity,
                propertyType.IsEnum ? Enum.ToObject(propertyType, value) : Convert.ChangeType(value, propertyType),
                null);
        }

        private void _setPropertyValue(object entity, PropertyInfo property, object value)
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
                propertyType = new System.ComponentModel.NullableConverter(property.PropertyType).UnderlyingType;
            }

            //use the converter to get the correct value
            property.SetValue(
                entity,
                propertyType.IsEnum ? Enum.ToObject(propertyType, value) : Convert.ChangeType(value, propertyType),
                null);
        }

        private void _setPropertyValue(object parent, object child, string propertyNameToSet)
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

                _setPropertyValue(child, foreignKeyAttribute.ForeignKeyColumnName, value);
            }
            else
            {
                var childPrimaryKey = child.GetPrimaryKeys().First();
                var value = child.GetType().GetProperty(childPrimaryKey.Name).GetValue(child);

                _setPropertyValue(parent, foreignKeyAttribute.ForeignKeyColumnName, value);
            }
        }

        private void _getSaveOrder<T>(T entity,
            List<ForeignKeySaveNode> savableObjects, bool isMARSEnabled)
            where T : class
        {
            var entities = _getForeignKeys(entity);

            entities.Insert(0, new ParentChildPair(null, entity, null));

            for (var i = 0; i < entities.Count; i++)
            {
                if (i == 0)
                {
                    // is the base entity, will never have a parent, set it and continue to the next entity
                    savableObjects.Add(new ForeignKeySaveNode(null, entity, null));
                    continue;
                }

                var e = entities[i];
                int index;
                var foreignKeyIsList = e.Property.IsList();
                var tableInfo = new TableInfo(e.Value.GetTypeListCheck());

                // skip lookup tables
                if (tableInfo.IsLookupTable) continue;

                // skip children(foreign keys) if option is set
                if (tableInfo.IsReadOnly &&
                    tableInfo.GetReadOnlySaveOption() == ReadOnlySaveOption.Skip)
                    continue;

                if (e.Value == null)
                {
                    if (foreignKeyIsList) continue;

                    var columnName = e.ChildType.GetCustomAttribute<ForeignKeyAttribute>().ForeignKeyColumnName;
                    var isNullable = entity.GetType().GetProperty(columnName).PropertyType.IsNullable();

                    // we can skip the foreign key if its nullable and one to one
                    if (isNullable) continue;

                    if (!isMARSEnabled)
                    {
                        // database will take care of this if MARS is enabled
                        // list can be one-many or one-none.  We assume the key to the primary table is in this table therefore the base table can still be saved while
                        // maintaining the relationship
                        throw new SqlSaveException(
                            string.Format(
                                "Foreign Key Has No Value - Foreign Key Property Name: {0}.  If the ForeignKey is nullable, make the ID nullable in the POCO to save",
                                e.GetType().Name));
                    }
                }

                // Check for readonly attribute and see if we should throw an error
                if (tableInfo.IsReadOnly &&
                    tableInfo.GetReadOnlySaveOption() == ReadOnlySaveOption.ThrowException)
                {
                    throw new SqlSaveException(string.Format(
                        "Table Is ReadOnly.  Table: {0}.  Change ReadOnlySaveOption to Skip if you wish to skip this table and its foreign keys",
                        entity.GetTableName()));
                }

                // doesnt have dependencies
                if (foreignKeyIsList)
                {
                    // e.Value can not be null, above code will catch it
                    foreach (var item in (e.Value as ICollection))
                    {
                        // make sure there are no saving issues only if MARS is disabled
                        if (!isMARSEnabled) _getState(item);

                        // ForeignKeySaveNode implements IEquatable and Overrides get hash code to only compare
                        // the value property
                        index = savableObjects.IndexOf(new ForeignKeySaveNode(null, e.Parent, null));

                        savableObjects.Insert(index + 1, new ForeignKeySaveNode(e.Property, item, e.Parent));

                        if (item.HasForeignKeys()) entities.AddRange(_getForeignKeys(item));
                    }
                }
                else
                {
                    // make sure there are no saving issues
                    if (!isMARSEnabled) _getState(e.Value);

                    // must be saved before the parent
                    // ForeignKeySaveNode implements IEquatable and Overrides get hash code to only compare
                    // the value property
                    index = savableObjects.IndexOf(new ForeignKeySaveNode(null, e.Parent, null));

                    savableObjects.Insert(index, new ForeignKeySaveNode(e.Property, e.Value, e.Parent));

                    // has dependencies
                    if (e.Value.HasForeignKeys()) entities.AddRange(_getForeignKeys(e.Value));
                }
            }
        }

        private List<ParentChildPair> _getForeignKeys(object entity)
        {
            return entity.GetForeignKeys()
                .OrderBy(w => w.PropertyType.IsList())
                .Select(w => new ParentChildPair(entity, w.GetValue(entity), w))
                .ToList();
        }

        #region helpers
        class ParentChildPair
        {
            public ParentChildPair(object parent, object value, PropertyInfo property)
            {
                Parent = parent;
                Value = value;
                Property = property;
            }

            public object Parent { get; private set; }

            public PropertyInfo Property { get; private set; }

            public Type ParentType
            {
                get { return Parent == null ? null : Parent.GetTypeListCheck(); }
            }

            public object Value { get; private set; }

            public Type ChildType
            {
                get { return Value == null ? null : Value.GetTypeListCheck(); }
            }
        }
        #endregion
    }
}
