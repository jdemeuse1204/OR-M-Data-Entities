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
using System.Linq;
using System.Reflection;
using OR_M_Data_Entities.Data.Definition;
using OR_M_Data_Entities.Data.Execution;
using OR_M_Data_Entities.Data.Query;
using OR_M_Data_Entities.Data.Secure;
using OR_M_Data_Entities.Enumeration;
using OR_M_Data_Entities.Exceptions;
using OR_M_Data_Entities.Extensions;
using OR_M_Data_Entities.Mapping;
using OR_M_Data_Entities.Tracking;

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
            var parent = new Entity(entity);

            // create our entity info analyzer
            var entityItems = parent.HasForeignKeys()
                ? parent.GetSaveOrder(Configuration.UseTransactions)
                : new EntitySaveNodeList(new ForeignKeySaveNode(null, parent, null));

            for (var i = 0; i < entityItems.Count; i++)
            {
                var entityItem = entityItems[i];
                ISqlBuilder builder;

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
                if (entityItem.Entity.IsEntityStateTrackingOn) EntityStateAnalyzer.TrySetPristineEntity(entity);
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

        private class SqlModificationBuilder : SqlNonTransactionUpdateBuilder
        {
            // Builders all should inherit from ISqlBuilder, should not be astract, base class should have the Build command in it
            public SqlModificationBuilder(Entity entity) 
                : base(entity)
            {
            }


        }

        private abstract class SqlExecutionPlan : ISqlBuilder
        {
            #region Constructor
            protected SqlExecutionPlan(Entity entity)
            {
                Entity = entity;
            }
            #endregion

            #region Properties and Fields
            public readonly Entity Entity;
            #endregion

            #region Methods
            public virtual List<ModifcationItem> GetModifcationItems()
            {
                return Entity.GetColumns().Select(property => new ModifcationItem(property, Entity)).ToList();
            }

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
                Fields = string.Empty;
                Values = string.Empty;
                Declare = string.Empty;
                Keys = string.Empty;
                SelectColumns = string.Empty;
                Where = string.Empty;
                Set = string.Empty;
                Update = string.Empty;
                FormattedTableName = plan.Entity.SqlFormattedTableName();
                ModificationItems = plan.GetModifcationItems();
            }

            #endregion

            #region Properties

            protected readonly List<ModifcationItem> ModificationItems;

            protected readonly string FormattedTableName;

            protected string Fields { get; set; }

            protected string Values { get; set; }

            protected string Declare { get; set; }

            protected readonly string Select = "SELECT TOP 1 {0}{1}";

            protected readonly string From = " FROM [{0}] WHERE {1}";

            protected string Keys { get; set; }

            protected string SelectColumns { get; set; }

            protected string Where { get; set; }

            protected string Set { get; set; }

            protected string Update { get; set; }

            protected bool DoSelectFromForKeyContainer { get; set; }

            #endregion

            #region Methods

            public abstract void CreatePackage();

            public abstract string GetSql();

            #endregion
        }
        #endregion

        #region Insert

        #region Non Transaction Builders

        private class SqlNonTransactionInsertBuilder : SqlExecutionPlan
        {
            public SqlNonTransactionInsertBuilder(Entity entity)
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
            public SqlNonTransactionTryInsertBuilder(Entity entity) 
                : base(entity)
            {
            }

            public override ISqlPackage Build()
            {
                var package = new SqlNonTransactionTryInsertPackage(this);

                package.CreatePackage();

                return package;
            }
        }

        private class SqlNonTransactionTryInsertUpdateBuilder : SqlExecutionPlan
        {
            public SqlNonTransactionTryInsertUpdateBuilder(Entity entity) 
                : base(entity)
            {
            }

            public override ISqlPackage Build()
            {
                var package = new SqlNonTransactionTryInsertUpdatePackage(this);

                package.CreatePackage();

                return package;
            }
        }

        #endregion

        #region Non Transaction Packages

        private class SqlNonTransactionInsertPackage : SqlModificationPackage
        {
            #region Constructor

            public SqlNonTransactionInsertPackage(SqlExecutionPlan builder) 
                : base(builder)
            {
            }

            #endregion

            #region Methods

            public override void CreatePackage()
            {
                if (ModificationItems.Count == 0) throw new QueryNotValidException("INSERT statement needs VALUES");

                DoSelectFromForKeyContainer = ModificationItems.Any(w => w.DbTranslationType == SqlDbType.Timestamp) || ModificationItems.Any(w => w.Generation == DbGenerationOption.DbDefault);

                for (var i = 0; i < ModificationItems.Count; i++)
                {
                    var item = ModificationItems[i];

                    //  NOTE:  Alias any Identity specification and generate columns with their property
                    // name not db column name so we can set the property when we return the values back.
                    switch (item.Generation)
                    {
                        case DbGenerationOption.None:
                            {
                                if (item.DbTranslationType == SqlDbType.Timestamp)
                                {
                                    SelectColumns += item.PropertyName == item.DatabaseColumnName ? string.Format("[{0}],", item.DatabaseColumnName) : string.Format("[{0}] as [{1}],", item.DatabaseColumnName, item.PropertyName);
                                    continue;
                                }

                                //Value is simply inserted

                                var data = item.TranslateDataType ? AddParameter(item.DatabaseColumnName, item.Value, item.DbTranslationType) : AddParameter(item.DatabaseColumnName, item.Value);

                                Fields += string.Format("[{0}],", item.DatabaseColumnName);
                                Values += string.Format("{0},", data);

                                if (!item.IsPrimaryKey)
                                {
                                    // should never update the pk
                                    Update += string.Format("[{0}] = {1},", item.DatabaseColumnName, data);
                                    continue;
                                }

                                Where += string.Format(string.IsNullOrEmpty(Where) ? "[{0}] = {1} " : "AND [{0}] = {1} ", item.DatabaseColumnName, data);
                            }
                            break;
                        case DbGenerationOption.Generate:
                            {
                                // Value is generated from the database
                                var key = string.Format("@{0}", item.PropertyName);

                                // make our set statement
                                if (item.SqlDataTypeString.ToUpper() == "UNIQUEIDENTIFIER")
                                {
                                    // GUID
                                    Set += string.Format("SET {0} = NEWID();", key);
                                }
                                else
                                {
                                    // INTEGER
                                    Set += string.Format("SET {0} = (Select ISNULL(MAX([{1}]),0) + 1 From [{2}]);", key, item.DatabaseColumnName, FormattedTableName);
                                }

                                Fields += string.Format("[{0}],", item.DatabaseColumnName);
                                Values += string.Format("{0},", key);
                                Declare += string.Format("{0} as {1},", key, item.SqlDataTypeString);
                                Keys += string.Format("{0} as [{1}],", key, item.PropertyName);
                                SelectColumns += item.PropertyName == item.DatabaseColumnName ? string.Format("[{0}],", item.DatabaseColumnName) : string.Format("[{0}] as [{1}],", item.DatabaseColumnName, item.PropertyName);

                                if (!item.IsPrimaryKey)
                                {
                                    var data = FindParameterKey(item.DatabaseColumnName);

                                    if (string.IsNullOrEmpty(data))
                                    {
                                        data = item.TranslateDataType ? AddParameter(item.DatabaseColumnName, item.Value, item.DbTranslationType) : AddParameter(item.DatabaseColumnName, item.Value);
                                    }

                                    Update += string.Format("[{0}] = {1},", item.DatabaseColumnName, data);
                                    continue;
                                }

                                Where += string.Format(string.IsNullOrEmpty(Where) ? "[{0}] = {1} " : "AND [{0}] = {1} ", item.DatabaseColumnName, key);

                                // Do not add as a parameter because the parameter will be converted to a string to
                                // be inserted in to the database
                            }
                            break;
                        case DbGenerationOption.IdentitySpecification:
                            {
                                SelectColumns += item.PropertyName == item.DatabaseColumnName ? string.Format("[{0}],", item.DatabaseColumnName) : string.Format("[{0}] as [{1}],", item.DatabaseColumnName, item.PropertyName);
                                Keys += string.Format("@@IDENTITY as [{0}],", item.PropertyName);

                                if (!item.IsPrimaryKey) continue;

                                Where += string.Format(string.IsNullOrEmpty(Where) ? "[{0}] = @@IDENTITY " : "AND [{0}] = @@IDENTITY ", item.DatabaseColumnName, item.DatabaseColumnName);
                            }
                            break;
                        case DbGenerationOption.DbDefault:
                            {
                                SelectColumns += item.PropertyName == item.DatabaseColumnName ? string.Format("[{0}],", item.DatabaseColumnName) : string.Format("[{0}] as [{1}],", item.DatabaseColumnName, item.PropertyName);
                                Keys += item.PropertyName == item.DatabaseColumnName ? string.Format("[{0}],", item.DatabaseColumnName) : string.Format("[{0}] as [{1}],", item.DatabaseColumnName, item.PropertyName);
                            }
                            break;
                    }
                }
            }

            public override string GetSql()
            {
                return string.Format("{0} {1} INSERT INTO [{2}] ({3}) VALUES ({4});{5}", string.IsNullOrWhiteSpace(Declare) ? string.Empty : string.Format("DECLARE {0}", Declare.TrimEnd(',')), Set, FormattedTableName, Fields.TrimEnd(','), Values.TrimEnd(','), SelectColumns.Any() ? DoSelectFromForKeyContainer ? string.Format(Select, SelectColumns.TrimEnd(','), string.Format(From, FormattedTableName, Where)) : string.Format(Select, Keys.TrimEnd(','), string.Empty) : string.Empty

                    // we want to select everything back from the database in case the model relies on db generation for some fields.
                    // this way we can load the data back into the model.  Works perfect for things like time stamps and auto generation
                    // where the column is not the PK
                    );
            }

            #endregion
        }

        private class SqlNonTransactionTryInsertPackage : SqlNonTransactionInsertPackage
        {
            public SqlNonTransactionTryInsertPackage(SqlExecutionPlan builder) : base(builder)
            {
            }

            public override string GetSql()
            {
                return string.Format(@"
{0}
{1}
IF (NOT(EXISTS(SELECT TOP 1 1 FROM [{2}] WHERE {6}))) 
    BEGIN
        INSERT INTO [{2}] ({3}) VALUES ({4});{5}
    END

", string.IsNullOrWhiteSpace(Declare) ? string.Empty : string.Format("DECLARE {0}", Declare.TrimEnd(',')), Set, FormattedTableName, Fields.TrimEnd(','), Values.TrimEnd(','), SelectColumns.Any() ? DoSelectFromForKeyContainer ? string.Format(Select, SelectColumns.TrimEnd(','), string.Format(From, FormattedTableName, Where)) : string.Format(Select, Keys.TrimEnd(','), string.Empty) : string.Empty,
                    // we want to select everything back from the database in case the model relies on db generation for some fields.
                    // this way we can load the data back into the model.  Works perfect for things like time stamps and auto generation
                    // where the column is not the PK
                    Where);
            }
        }

        private class SqlNonTransactionTryInsertUpdatePackage : SqlNonTransactionInsertPackage
        {
            public SqlNonTransactionTryInsertUpdatePackage(SqlExecutionPlan builder) : base(builder)
            {
            }

            public override string GetSql()
            {
                return string.Format(@"
{0}
{1}
IF (NOT(EXISTS(SELECT TOP 1 1 FROM [{2}] WHERE {6}))) 
    BEGIN
        INSERT INTO [{2}] ({3}) VALUES ({4});{5}
    END
ELSE
    BEGIN
        UPDATE [{2}] SET {7} WHERE {6}
    END
", string.IsNullOrWhiteSpace(Declare) ? string.Empty : string.Format("DECLARE {0}", Declare.TrimEnd(',')), Set, FormattedTableName, Fields.TrimEnd(','), Values.TrimEnd(','), SelectColumns.Any() ? DoSelectFromForKeyContainer ? string.Format(Select, SelectColumns.TrimEnd(','), string.Format(From, FormattedTableName, Where)) : string.Format(Select, Keys.TrimEnd(','), string.Empty) : string.Empty,
                    // we want to select everything back from the database in case the model relies on db generation for some fields.
                    // this way we can load the data back into the model.  Works perfect for things like time stamps and auto generation
                    // where the column is not the PK
                    Where, Update.TrimEnd(','));
            }
        }

        #endregion

        #endregion

        #region Update

        #region Non Transaction Builders

        private class SqlNonTransactionUpdateBuilder : SqlExecutionPlan
        {
            public SqlNonTransactionUpdateBuilder(Entity entity)
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

            public override void CreatePackage()
            {
                // there must be columns in the entity
                if (ModificationItems.Count == 0) throw new QueryNotValidException("UPDATE statement needs items to SET");

                Update = string.Format("UPDATE [{0}] SET ", FormattedTableName);

                // skip anything not modified
                var updateItems = ModificationItems.Where(w => w.IsModified).ToList();

                // if we got here there are columns to update, the entity is analyzed before this method.  Check again anyways
                if (updateItems.Count == 0) throw new QueryNotValidException("No items to update, query analyzer failed");

                for (var i = 0; i < updateItems.Count; i++)
                {
                    var item = ModificationItems[i];
                    var parameter = AddParameter(item.DatabaseColumnName, item.Value);

                    Update += string.Format("[{0}] = {1},", item.DatabaseColumnName, parameter);
                }

                Update = Update.TrimEnd(',');

                var primaryKeys = ModificationItems.Where(w => w.IsPrimaryKey).ToList();
                var where = " WHERE ";

                // add where statement
                for (var i = 0; i < primaryKeys.Count; i++)
                {
                    var key = primaryKeys[i];
                    var parameter = AddParameter(key.DatabaseColumnName, key.Value);

                    where += string.Format("[{0}] = {1}", key.DatabaseColumnName, parameter);
                }

                Update += where;
            }

            public override string GetSql()
            {
                return Update;
            }

            #endregion
        }

        #endregion

        #endregion
    }
}
