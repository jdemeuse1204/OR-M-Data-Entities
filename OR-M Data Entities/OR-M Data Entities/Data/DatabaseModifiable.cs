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
using OR_M_Data_Entities.Data.Definition;
using OR_M_Data_Entities.Data.Modification;
using OR_M_Data_Entities.Data.Query;
using OR_M_Data_Entities.Data.Secure;
using OR_M_Data_Entities.Enumeration;
using OR_M_Data_Entities.Exceptions;
using OR_M_Data_Entities.Extensions;
using OR_M_Data_Entities.Mapping;

namespace OR_M_Data_Entities.Data
{
    /// <summary>
    /// This class uses DataFetching functions to Save, Delete
    /// </summary>
    public abstract partial class DatabaseModifiable : DatabaseFetching
    {
        #region Events And Delegates

        public delegate void OnBeforeSaveHandler(object entity, UpdateType updateType);

        public event OnBeforeSaveHandler OnBeforeSave;

        public delegate void OnAfterSaveHandler(object entity, UpdateType actualUpdateType);

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

        #endregion

        #region Save Methods

        public virtual bool SaveChanges<T>(T entity)
            where T : class
        {
            return Configuration.UseTransactions ? _saveChangesUsingTransactions(entity) : _saveChanges(entity);
        }

        public virtual bool _saveChangesUsingTransactions<T>(T entity)
            where T : class
        {
            var saves = new List<UpdateType>();
            var parent = new ModificationEntity(entity);
            var builders = new List<ISqlBuilder>();

            // get all items to save and get them in order
            var entityItems = _getSaveItems(parent);

            for (var i = 0; i < entityItems.Count; i++)
            {
                var entityItem = entityItems[i];
                ISqlBuilder builder;

                // add the save to the list so we can tell the user what the save action did
                saves.Add(entityItem.Entity.UpdateType);

                if (OnBeforeSave != null) OnBeforeSave(entityItem.Entity.Value, entityItem.Entity.UpdateType);

                // Get the correct execution plan
                switch (entityItem.Entity.UpdateType)
                {
                    case UpdateType.Insert:
                        builder = new SqlInsertBuilder(entityItem.Entity);
                        break;
                    case UpdateType.TryInsert:
                        builder = new SqlTryInsertBuilder(entityItem.Entity);
                        break;
                    case UpdateType.TryInsertUpdate:
                        builder = new SqlTryInsertUpdateBuilder(entityItem.Entity);
                        break;
                    case UpdateType.Update:
                        builder = new SqlUpdateBuilder(entityItem.Entity);
                        break;
                    case UpdateType.Skip:
                        continue;
                    default:
                        throw new ArgumentOutOfRangeException();
                }

                builders.Add(builder);

                // execute transaction
                // make a transaction builder because they will be so different
            }

            return saves.Any(w => w != UpdateType.Skip);
        }

        /// <summary>
        /// returns true if anything was modified and false if no changes were made
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="entity"></param>
        /// <returns></returns>
        public virtual bool _saveChanges<T>(T entity)
            where T : class
        {
            try
            {
                var saves = new List<UpdateType>();
                var parent = new ModificationEntity(entity);

                // get all items to save and get them in order
                var entityItems = _getSaveItems(parent);

                for (var i = 0; i < entityItems.Count; i++)
                {
                    var entityItem = entityItems[i];
                    ISqlBuilder builder;

                    // add the save to the list so we can tell the user what the save action did
                    saves.Add(entityItem.Entity.UpdateType);

                    if (OnBeforeSave != null) OnBeforeSave(entityItem.Entity.Value, entityItem.Entity.UpdateType);

                    // Get the correct execution plan
                    switch (entityItem.Entity.UpdateType)
                    {
                        case UpdateType.Insert:
                            builder = new SqlInsertBuilder(entityItem.Entity);
                            break;
                        case UpdateType.TryInsert:
                            builder = new SqlTryInsertBuilder(entityItem.Entity);
                            break;
                        case UpdateType.TryInsertUpdate:
                            builder = new SqlTryInsertUpdateBuilder(entityItem.Entity);
                            break;
                        case UpdateType.Update:
                            builder = new SqlUpdateBuilder(entityItem.Entity);
                            break;
                        case UpdateType.Skip:
                            continue;
                        default:
                            throw new ArgumentOutOfRangeException();
                    }

                    // If relationship is one-many.  Need to set the foreign key before saving
                    if (entityItem.Parent != null && entityItem.Property.IsList())
                    {
                        Entity.SetPropertyValue(entityItem.Parent, entityItem.Entity.Value, entityItem.Property.Name);
                    }

                    if (OnSaving != null) OnSaving(entityItem.Entity.Value);

                    // execute the sql
                    ExecuteReader(builder);

                    var keyContainer = GetOutput();

                    // check if a concurrency violation has occurred
                    if (entityItem.Entity.UpdateType == UpdateType.Update && keyContainer.Count == 0 &&
                        Configuration.ConcurrencyViolationRule == ConcurrencyViolationRule.ThrowException)
                    {
                        throw new DBConcurrencyException("Concurrency Violation.  {0} was changed prior to this update");
                    }

                    // put updated values into entity
                    foreach (var item in keyContainer)
                    {
                        // find the property first in case the column name change attribute is used
                        // Key is property name, value is the db value
                        entityItem.Entity.SetPropertyValue(
                            item.Key,
                            item.Value);
                    }

                    // If relationship is one-one.  Need to set the foreign key after saving
                    if (entityItem.Parent != null && !entityItem.Property.IsList())
                    {
                        Entity.SetPropertyValue(entityItem.Parent, entityItem.Entity.Value, entityItem.Property.Name);
                    }

                    // set the pristine state only if entity tracking is on
                    if (entityItem.Entity.IsEntityStateTrackingOn) ModificationEntity.TrySetPristineEntity(entityItem.Entity.Value);

                    if (OnAfterSave != null) OnAfterSave(entityItem.Entity.Value, entityItem.Entity.UpdateType);
                }

                return saves.Any(w => w != UpdateType.Skip);
            }
            catch (MaxLengthException ex)
            {
                // only catch the max length exception so we can tell the user that the save was cancelled
                throw new SqlSaveException("Max length violated, see inner exception", ex);
            }
        }

        #endregion

        #region Delete Methods

        public virtual bool Delete<T>(T entity) where T : class
        {
            var saves = new List<UpdateType>();
            var parent = new DeleteEntity(entity);

            // get all items to save and get them in order
            var entityItems = _getSaveItems(parent);

            // reverse the order to back them out of the database
            entityItems.Reverse();

            for (var i = 0; i < entityItems.Count; i++)
            {
                var entityItem = entityItems[i];

                if (OnBeforeSave != null) OnBeforeSave(entityItem.Entity.Value, UpdateType.Delete);

                var builder = new SqlDeleteBuilder(entityItem.Entity);

                if (OnSaving != null) OnSaving(entityItem.Entity.Value);

                // execute the sql
                ExecuteReader(builder);

                // we return the deleted id's to check and see if anything was deleted
                var keyContainer = GetOutput();
                var actionTaken = keyContainer.Count > 0 ? UpdateType.Delete : UpdateType.RowNotFound;

                // add the save to the list so we can tell the user what the save action did
                saves.Add(actionTaken);

                // set the pristine state only if entity tracking is on
                if (entityItem.Entity.IsEntityStateTrackingOn) ModificationEntity.TrySetPristineEntity(entityItem.Entity.Value);

                if (OnAfterSave != null) OnAfterSave(entityItem.Entity.Value, actionTaken);
            }

            return saves.Any(w => w == UpdateType.Delete);
        }

        #endregion

        #region Methods

        private EntitySaveNodeList _getSaveItems(ModificationEntity entity)
        {
            return entity.HasForeignKeys()
                ? _getSaveOrder(entity, Configuration.UseTransactions)
                : new EntitySaveNodeList(new Node(entity));
        }

        private EntitySaveNodeList _getSaveOrder(ModificationEntity entity, bool useTransactions)
        {
            var result = new EntitySaveNodeList();

            var entities = Entity.GetForeignKeys(entity.Value);

            entities.Insert(0, new ForeignKeyAssociation(null, entity.Value, null));

            for (var i = 0; i < entities.Count; i++)
            {
                if (i == 0)
                {
                    // is the base entity, will never have a parent, set it and continue to the next entity
                    result.Add(new Node(entity));
                    continue;
                }

                var e = entities[i];
                int index;
                var foreignKeyIsList = e.Property.IsList();
                var tableInfo = new Table(e.Property.GetPropertyType());

                if (e.Value == null && !useTransactions)
                {
                    throw new SqlSaveException(string.Format("Foreign Key [{0}] cannot be null", tableInfo.TableNameOnly));
                }

                // skip lookup tables
                if (tableInfo.IsLookupTable) continue;

                // skip children(foreign keys) if option is set
                if (tableInfo.IsReadOnly && tableInfo.GetReadOnlySaveOption() == ReadOnlySaveOption.Skip)
                    continue;

                if (e.Value == null)
                {
                    if (foreignKeyIsList) continue;

                    var columnName = e.ChildType.GetCustomAttribute<ForeignKeyAttribute>().ForeignKeyColumnName;
                    var isNullable = e.Parent.GetType().GetProperty(columnName).PropertyType.IsNullable();

                    // we can skip the foreign key if its nullable and one to one
                    if (isNullable) continue;

                    if (!useTransactions)
                    {
                        // database will take care of this if MARS is enabled
                        // list can be one-many or one-none.  We assume the key to the primary table is in this table therefore the base table can still be saved while
                        // maintaining the relationship
                        throw new SqlSaveException(string.Format("Foreign Key Has No Value - Foreign Key Property Name: {0}.  If the ForeignKey is nullable, make the ID nullable in the POCO to save", e.GetType().Name));
                    }
                }

                // Check for readonly attribute and see if we should throw an error
                if (tableInfo.IsReadOnly && tableInfo.GetReadOnlySaveOption() == ReadOnlySaveOption.ThrowException)
                {
                    throw new SqlSaveException(string.Format("Table Is ReadOnly.  Table: {0}.  Change ReadOnlySaveOption to Skip if you wish to skip this table and its foreign keys", tableInfo.GetTableName()));
                }

                // doesnt have dependencies
                if (foreignKeyIsList)
                {
                    // e.Value can not be null, above code will catch it
                    foreach (var item in (e.Value as ICollection))
                    {
                        // ForeignKeySaveNode implements IEquatable and Overrides get hash code to only compare
                        // the value property
                        index = result.IndexOf(e.Parent);

                        result.Insert(index + 1, new Node(item, e.Property, e.Parent));

                        if (item.HasForeignKeys()) entities.AddRange(Entity.GetForeignKeys(item));
                    }
                }
                else
                {
                    // must be saved before the parent
                    // ForeignKeySaveNode implements IEquatable and Overrides get hash code to only compare
                    // the value property
                    index = result.IndexOf(e.Parent);

                    result.Insert(index, new Node(e.Value, e.Property, e.Parent));

                    // has dependencies
                    if (e.Value.HasForeignKeys()) entities.AddRange(Entity.GetForeignKeys(e.Value));
                }
            }

            return result;
        }
        #endregion

        #region shared
        private class EntitySaveNodeList : IEnumerable<Node>
        {
            #region Properties and Fields
            public int Count
            {
                get { return _internal.Count; }
            }

            public Node this[int i]
            {
                get { return _internal[i] as Node; }
            }

            private readonly List<object> _internal;
            #endregion

            #region Constructor
            public EntitySaveNodeList()
            {
                _internal = new List<object>();
            }

            public EntitySaveNodeList(Node node)
                : this()
            {
                _internal.Add(node);
            }
            #endregion

            #region Methods
            public int IndexOf(object entity)
            {
                return _internal.IndexOf(entity);
            }

            public void Insert(int index, Node node)
            {
                _internal.Insert(index, node);
            }

            public void Add(Node node)
            {
                _internal.Add(node);
            }

            public void Reverse()
            {
                _internal.Reverse();
            }

            public IEnumerator<Node> GetEnumerator()
            {
                return ((dynamic)_internal).GetEnumerator();
            }

            IEnumerator IEnumerable.GetEnumerator()
            {
                return GetEnumerator();
            }
            #endregion
        }

        private sealed class Node : IEquatable<Node>
        {
            public Node(object value, PropertyInfo property = null, object parent = null)
                : this(new ModificationEntity(value), property, parent)
            {
            }

            public Node(ModificationEntity entity, PropertyInfo property = null, object parent = null)
            {
                Property = property;
                Parent = parent;
                Entity = entity;
            }

            public PropertyInfo Property { get; private set; }

            public object Parent { get; private set; }

            public readonly ModificationEntity Entity;

            public bool Equals(Node other)
            {
                //Check whether the compared object is null.
                if (ReferenceEquals(other, null)) return false;

                //Check whether the compared object references the same data.
                if (ReferenceEquals(this, other)) return true;

                //Check whether the products' properties are equal.
                return Entity == other.Entity;
            }

            public override bool Equals(object obj)
            {
                return Entity.Equals(obj);
            }

            // If Equals() returns true for a pair of objects 
            // then GetHashCode() must return the same value for these objects.
            public override int GetHashCode()
            {
                //Calculate the hash code for the product.
                return Entity.Value.GetHashCode();
            }
        }

        private abstract class SqlSecureExecutable
        {
            #region Fields
            private readonly List<SqlSecureQueryParameter> _parameters;
            #endregion

            #region Constructor
            protected SqlSecureExecutable()
            {
                _parameters = new List<SqlSecureQueryParameter>();
            }

            protected SqlSecureExecutable(List<SqlSecureQueryParameter> parameters)
            {
                _parameters = parameters;
            }
            #endregion

            #region Methods
            // key where the data will be insert into the secure command
            private string _getNextKey()
            {
                return string.Format("@DATA{0}", _parameters.Count);
            }

            protected string AddParameter(ModificationItem item, object value)
            {
                return _addParameter(item, value, false);
            }

            protected string AddPristineParameter(ModificationItem item, object value)
            {
                return _addParameter(item, value, true);
            }

            private string _addParameter(ModificationItem item, object value, bool addPristineParameter)
            {
                var parameterKey = _getNextKey();

                _parameters.Add(new SqlSecureQueryParameter
                {
                    Key = parameterKey,
                    DbColumnName = addPristineParameter ? string.Format("Pristine{0}", item.DatabaseColumnName) : item.DatabaseColumnName,
                    TableName = item.GetTableName(),
                    ForeignKeyPropertyName = item.GetTableName(),
                    Value = item.TranslateDataType ? new SqlSecureObject(value, item.DbTranslationType) : new SqlSecureObject(value)
                });

                return parameterKey;
            }

            public void InsertParameters(SqlCommand cmd)
            {
                foreach (var item in _parameters)
                {
                    cmd.Parameters.Add(cmd.CreateParameter()).ParameterName = item.Key;
                    cmd.Parameters[item.Key].Value = item.Value.Value;

                    if (item.Value.TranslateDataType)
                    {
                        cmd.Parameters[item.Key].SqlDbType = item.Value.DbTranslationType;
                    }
                }
            }

            public IEnumerable<SqlSecureQueryParameter> GetParameters()
            {
                return _parameters;
            }

            protected string FindParameterKey(string dbColumnName)
            {
                var parameter = _parameters.FirstOrDefault(w => w.DbColumnName == dbColumnName);

                return parameter != null ? parameter.Key : null;
            }
            #endregion
        }
        #endregion
    }
}
