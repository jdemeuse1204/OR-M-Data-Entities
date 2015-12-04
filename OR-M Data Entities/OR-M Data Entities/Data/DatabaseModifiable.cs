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
using System.Data.SqlClient;
using OR_M_Data_Entities.Data.Definition;
using OR_M_Data_Entities.Data.Execution;
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

        #endregion

        #region Save Methods
        public virtual SaveResult SaveChanges<T>(T entity)
            where T : class
        {
            var saves = new List<KeyValuePair<string, UpdateType>>();
            var parent = new ModificationEntity(entity);

            // create our entity info analyzer
            var entityItems = parent.HasForeignKeys()
                ? parent.GetSaveOrder(Configuration.UseTransactions)
                : new EntitySaveNodeList(new ForeignKeySaveNode(null, parent, null));

            for (var i = 0; i < entityItems.Count; i++)
            {
                var entityItem = entityItems[i];
                ISqlBuilder builder;

                // add the save to the list so we can tell the user what the save action did
                saves.Add(new KeyValuePair<string, UpdateType>(entityItem.Entity.TableNameOnly, entityItem.Entity.UpdateType));

                // Get the correct execution plan
                switch (entityItem.Entity.UpdateType)
                {
                    case UpdateType.Insert:
                        builder = new SqlNonTransactionInsertBuilder(entityItem.Entity);
                        break;
                    case UpdateType.TryInsert:
                        builder = new SqlNonTransactionTryInsertBuilder(entityItem.Entity);
                        break;
                    case UpdateType.TryInsertUpdate:
                        builder = new SqlNonTransactionTryInsertUpdateBuilder(entityItem.Entity);
                        break;
                    case UpdateType.Update:
                        builder = new SqlNonTransactionUpdateBuilder(entityItem.Entity);
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

                // execute the sql
                ExecuteReader(builder);

                // put updated values into entity
                foreach (var item in SelectIdentity())
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
                if (entityItem.Entity.IsEntityStateTrackingOn) ModificationEntity.TrySetPristineEntity(entity);
            }

            return new SaveResult(saves);
        }

        #endregion

        #region Delete Methods

        public virtual bool Delete<T>(T entity) where T : class
        {
            return false;
        }

        #endregion
    }

    /// <summary>
    /// Created partial class to split off query builders.  
    /// We do not want the end user to know about any sql builders, we hide them all in the partial class
    /// </summary>
    public partial class DatabaseModifiable
    {
        /// <summary>
        /// Provides us a way to get the execution plan for an entity
        /// </summary>

        #region Base
        private class CustomContainer : SqlModificationContainer, ISqlContainer
        {
            private readonly string _sql;

            public CustomContainer(Table entity, string sql) 
                : base(entity)
            {
                _sql = sql;
            }

            public string Resolve()
            {
                return _sql;
            }
        }

        private class InsertContainer : SqlModificationContainer, ISqlContainer
        {
            #region Properties
            private string _fields { get; set; }

            private string _values { get; set; }

            private string _declare { get; set; }

            private string _output { get; set; }

            private string _set { get; set; }

            #endregion

            #region Constructor
            public InsertContainer(Table entity)
                : base(entity)
            {
                _fields = string.Empty;
                _values = string.Empty;
                _declare = string.Empty;
                _output = string.Empty;
                _set = string.Empty;
            }
            #endregion

            #region Methods
            public void AddField(ModificationItem item)
            {
                _fields += item.AsField(",");
            }

            public void AddValue(string parameterKey)
            {
                _values += string.Format("{0},", parameterKey);
            }

            public void AddDeclare(string parameterKey, string sqlDataType)
            {
                _declare += string.Format("{0} as {1},", parameterKey, sqlDataType);
            }

            public void AddOutput(ModificationItem item)
            {
                _output += item.AsOutput(",");
            }

            public void AddSet(ModificationItem item, out string key)
            {
                key = string.Format("@{0}", item.PropertyName);

                // make our set statement
                if (item.SqlDataTypeString.ToUpper() == "UNIQUEIDENTIFIER")
                {
                    // GUID
                    _set += string.Format("SET {0} = NEWID();", key);
                }
                else
                {
                    // INTEGER
                    _set += string.Format("SET {0} = (Select ISNULL(MAX([{1}]),0) + 1 From [{2}]);", key, item.DatabaseColumnName, SqlFormattedTableName);
                }
            }

            public string Resolve()
            {
                return string.Format("{0} {1} INSERT INTO [{2}] ({3}) OUTPUT {5} VALUES ({4})",
                    string.IsNullOrWhiteSpace(_declare)
                        ? string.Empty
                        : string.Format("DECLARE {0}", _declare.TrimEnd(',')),
                    _set,
                    SqlFormattedTableName,
                    _fields.TrimEnd(','),
                    _values.TrimEnd(','),
                    _output.TrimEnd(',')

                    // we want to select everything back from the database in case the model relies on db generation for some fields.
                    // this way we can load the data back into the model.  Works perfect for things like time stamps and auto generation
                    // where the column is not the PK
                    );
            }
            #endregion
        }

        private class UpdateContainer : SqlModificationContainer, ISqlContainer
        {
            private string _setItems { get; set; }

            private string _where { get; set; }

            private readonly string _statement;

            public UpdateContainer(ModificationEntity entity)
                : base(entity)
            {
                _setItems = string.Empty;
                _where = string.Empty;
                _statement = string.Format("UPDATE [{0}]", SqlFormattedTableName);
            }

            public void AddUpdate(ModificationItem item, string parameterKey)
            {
                _setItems += string.Format("[{0}] = {1},", item.DatabaseColumnName, parameterKey);
            }

            public void AddWhere(ModificationItem item, string parameterKey)
            {
                _where += string.Format("[{0}] = {1}", item.DatabaseColumnName, parameterKey);
            }

            public string Resolve()
            {
                return string.Format("{0} SET {1} WHERE {2}", _statement, _setItems.TrimEnd(','), _where.TrimEnd(','));
            }
        }

        protected abstract class SqlModificationContainer
        {
            protected SqlModificationContainer(Table entity)
            {
                SqlFormattedTableName = entity.SqlFormattedTableName();
            }

            protected readonly string SqlFormattedTableName;
        }

        private class SqlModificationBuilder : SqlNonTransactionUpdateBuilder
        {
            // Builders all should inherit from ISqlBuilder, should not be astract, base class should have the Build command in it
            public SqlModificationBuilder(ModificationEntity entity)
                : base(entity)
            {
            }
        }

        private abstract class SqlExecutionPlan : ISqlBuilder
        {
            #region Constructor
            protected SqlExecutionPlan(ModificationEntity entity)
            {
                Entity = entity;
            }
            #endregion

            #region Properties and Fields
            public readonly ModificationEntity Entity;
            #endregion

            #region Methods
            public abstract ISqlPackage Build();

            public SqlCommand BuildSqlCommand(SqlConnection connection)
            {
                // build the sql package
                var package = Build();

                // generate the sql command
                var command = new SqlCommand(package.GetSql(), connection);

                // insert the parameters
                package.InsertParameters(command);

                return command;
            }

            #endregion
        }

        private abstract class SqlModificationPackage : SqlSecureExecutable, ISqlPackage
        {
            #region Constructor
            protected SqlModificationPackage(SqlExecutionPlan plan)
            {
                Entity = plan.Entity;
            }

            #endregion

            #region Properties
            protected readonly ModificationEntity Entity;
            #endregion

            #region Methods

            public abstract ISqlContainer CreatePackage();

            public string GetSql()
            {
                var container = CreatePackage();

                return container.Resolve();
            }

            #endregion
        }
        #endregion

        #region Insert

        #region Non Transaction Builders

        private class SqlNonTransactionInsertBuilder : SqlExecutionPlan
        {
            public SqlNonTransactionInsertBuilder(ModificationEntity entity)
                : base(entity)
            {
            }

            public override ISqlPackage Build()
            {
                var package = new SqlNonTransactionInsertPackage(this);

                package.CreatePackage();

                return package;
            }
        }

        private class SqlNonTransactionTryInsertBuilder : SqlExecutionPlan
        {
            public SqlNonTransactionTryInsertBuilder(ModificationEntity entity)
                : base(entity)
            {
            }

            public override ISqlPackage Build()
            {
                var insert = new SqlNonTransactionInsertPackage(this);
                var package = new SqlNonTransactionExistsPackage(this, insert);

                package.CreatePackage();

                return package;
            }
        }

        private class SqlNonTransactionTryInsertUpdateBuilder : SqlExecutionPlan
        {
            public SqlNonTransactionTryInsertUpdateBuilder(ModificationEntity entity)
                : base(entity)
            {
            }

            public override ISqlPackage Build()
            {
                var insert = new SqlNonTransactionInsertPackage(this);
                var update = new SqlNonTransactionUpdatePackage(this);

                var package = new SqlNonTransactionExistsPackage(this, insert, update);

                package.CreatePackage();

                return package;
            }
        }

        #endregion

        #region Non Transaction Packages

        private class SqlNonTransactionExistsPackage : SqlModificationPackage
        {
            private readonly SqlModificationPackage _exists;

            private readonly SqlModificationPackage _notExists;

            private readonly string _existsStatement;

            private string _where { get; set; }

            public SqlNonTransactionExistsPackage(SqlExecutionPlan plan, SqlModificationPackage exists, SqlModificationPackage notExists = null)
                : base(plan)
            {
                _exists = exists;
                _notExists = notExists;

                // keys are not part of changes so we need to grab them
                var primaryKeys = Entity.Keys();

                // add where statement
                for (var i = 0; i < primaryKeys.Count; i++)
                {
                    var key = primaryKeys[i];
                    var value = Entity.GetPropertyValue(key.PropertyName);
                    var parameter = AddParameter(key.DatabaseColumnName, value);

                    _addWhere(key, parameter);
                }

                _existsStatement = string.Format(@"IF (NOT(EXISTS(SELECT TOP 1 1 FROM [{0}] WHERE {1}))) 
                                        BEGIN
                                            {{2}}
                                        {3}", plan.Entity.SqlFormattedTableName(), 
                                        
                                        _where, 
                                        0,
                                        notExists == null ? 
"END" : 
@"ELSE 
    {1} 
END");
            }

            private void _addWhere(ModificationItem item, string parameterKey)
            {
                _where += string.Format("[{0}] = {1}", item.DatabaseColumnName, parameterKey);
            }

            public override ISqlContainer CreatePackage()
            {
                return _notExists != null
                    ? new CustomContainer(Entity, string.Format(_existsStatement, _exists.GetSql(), _notExists.GetSql()))
                    : new CustomContainer(Entity, string.Format(_existsStatement, _exists.GetSql()));
            }
        }


        private class SqlNonTransactionInsertPackage : SqlModificationPackage
        {
            #region Constructor

            public SqlNonTransactionInsertPackage(SqlExecutionPlan builder)
                : base(builder)
            {
            }

            #endregion

            #region Methods

            public override ISqlContainer CreatePackage()
            {
                var items = Entity.All();
                var container = new InsertContainer(Entity);

                if (items.Count == 0) throw new QueryNotValidException("INSERT statement needs VALUES");

                for (var i = 0; i < items.Count; i++)
                {
                    var item = items[i];

                    //  NOTE:  Alias any Identity specification and generate columns with their property
                    // name not db column name so we can set the property when we return the values back.
                    switch (item.Generation)
                    {
                        case DbGenerationOption.None:
                            {
                                if (item.DbTranslationType == SqlDbType.Timestamp)
                                {
                                    container.AddOutput(item);
                                    continue;
                                }

                                var value = Entity.GetPropertyValue(item.PropertyName);
                                //Value is simply inserted
                                var data = item.TranslateDataType
                                    ? AddParameter(item.DatabaseColumnName, value, item.DbTranslationType)
                                    : AddParameter(item.DatabaseColumnName, value);

                                container.AddField(item);
                                container.AddValue(data);
                                container.AddOutput(item);
                            }
                            break;
                        case DbGenerationOption.Generate:
                            {
                                // key from the set method
                                string key;

                                container.AddSet(item, out key);
                                container.AddField(item);
                                container.AddValue(key);
                                container.AddDeclare(key, item.SqlDataTypeString);
                                container.AddOutput(item);
                            }
                            break;
                        case DbGenerationOption.DbDefault:
                        case DbGenerationOption.IdentitySpecification:
                            {
                                container.AddOutput(item);
                            }
                            break;
                    }
                }

                return container;
            }
            #endregion
        }

        private class SqlNonTransactionTryInsertPackage : SqlNonTransactionInsertPackage
        {
            public SqlNonTransactionTryInsertPackage(SqlExecutionPlan builder)
                : base(builder)
            {
            }


        }

        private class SqlNonTransactionTryInsertUpdatePackage : SqlNonTransactionInsertPackage
        {
            public SqlNonTransactionTryInsertUpdatePackage(SqlExecutionPlan builder) : base(builder)
            {
            }


        }

        #endregion

        #endregion

        #region Update

        #region Non Transaction Builders

        private class SqlNonTransactionUpdateBuilder : SqlExecutionPlan
        {
            public SqlNonTransactionUpdateBuilder(ModificationEntity entity)
                : base(entity)
            {
            }

            public override ISqlPackage Build()
            {
                var package = new SqlNonTransactionUpdatePackage(this);

                package.CreatePackage();

                return package;
            }
        }

        #endregion

        #region Non Transaction Packages

        private class SqlNonTransactionUpdatePackage : SqlModificationPackage
        {
            #region Constructor

            public SqlNonTransactionUpdatePackage(SqlExecutionPlan builder)
                : base(builder)
            {
            }

            #endregion

            #region Methods
            public override ISqlContainer CreatePackage()
            {
                var items = Entity.Changes();
                var container = new UpdateContainer(Entity);

                // if we got here there are columns to update, the entity is analyzed before this method.  Check again anyways
                if (items.Count == 0) throw new SqlSaveException("No items to update, query analyzer failed");

                for (var i = 0; i < items.Count; i++)
                {
                    var item = items[i];
                    var value = Entity.GetPropertyValue(item.PropertyName);

                    var parameter = AddParameter(item.DatabaseColumnName, value);

                    container.AddUpdate(item, parameter);
                }

                // keys are not part of changes so we need to grab them
                var primaryKeys = Entity.Keys();

                // add where statement
                for (var i = 0; i < primaryKeys.Count; i++)
                {
                    var key = primaryKeys[i];
                    var value = Entity.GetPropertyValue(key.PropertyName);
                    var parameter = AddParameter(key.DatabaseColumnName, value);

                    container.AddWhere(key, parameter);
                }

                return container;
            }
            #endregion
        }

        #endregion

        #endregion
    }
}
