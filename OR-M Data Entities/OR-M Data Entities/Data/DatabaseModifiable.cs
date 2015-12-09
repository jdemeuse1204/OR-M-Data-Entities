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
using System.Linq;
using OR_M_Data_Entities.Data.Definition;
using OR_M_Data_Entities.Data.Execution;
using OR_M_Data_Entities.Data.Modification;
using OR_M_Data_Entities.Data.Query;
using OR_M_Data_Entities.Enumeration;
using OR_M_Data_Entities.Extensions;

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
                        builder = new SqlTransactionInsertBuilder(entityItem.Entity);
                        var b = builder.Build();
                        b.CreatePackage();
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

                if (OnAfterSave != null) OnAfterSave(entityItem.Entity.Value);
            }

            return saves.Any(w => w != UpdateType.Skip);
        }

        #endregion

        #region Delete Methods

        public virtual bool Delete<T>(T entity) where T : class
        {
            return false;
        }

        #endregion

        #region Methods

        private EntitySaveNodeList _getSaveItems(ModificationEntity entity)
        {
            return entity.HasForeignKeys()
                ? entity.GetSaveOrder(Configuration.UseTransactions)
                : new EntitySaveNodeList(new ForeignKeySaveNode(null, entity, null));
        } 
        #endregion
    }
}
