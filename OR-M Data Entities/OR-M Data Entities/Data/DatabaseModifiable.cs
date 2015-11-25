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
        public virtual UpdateType SaveChanges<T>(T entity)
            where T : class
        {
            var entityInfo = new EntityInfo(entity);
            var executionPlan = new SqlExecutionPlan(entityInfo);

            // Get the correct execution plan
            switch (executionPlan.UpdateType)
            {
                case UpdateType.Insert:
                    executionPlan = (SqlNonTransactionInsertBuilder)executionPlan;
                    break;
                case UpdateType.TryInsert:
                    executionPlan = (SqlNonTransactionTryInsertBuilder)executionPlan;
                    break;
                case UpdateType.TryInsertUpdate:
                    executionPlan = (SqlNonTransactionTryInsertUpdateBuilder)executionPlan;
                    break;
                case UpdateType.Update:
                    executionPlan = (SqlNonTransactionUpdateBuilder)executionPlan;
                    break;
                case UpdateType.Skip:
                    executionPlan = (SqlSkipModificationPackage)executionPlan;
                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }

            ExecuteReader((ISqlBuilder)executionPlan);

            return UpdateType.Insert;
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

        private class ModifcationItem
        {
            #region Properties

            public string SqlDataTypeString { get; private set; }

            public string PropertyDataType { get; private set; }

            public string PropertyName { get; private set; }

            public string DatabaseColumnName { get; private set; }

            public string KeyName { get; private set; }

            public SqlDbType DbTranslationType { get; private set; }

            public bool IsPrimaryKey { get; private set; }

            public DbGenerationOption Generation { get; private set; }

            public object Value { get; private set; }

            public bool TranslateDataType { get; private set; }

            #endregion

            #region Constructor

            public ModifcationItem(PropertyInfo property, object entity)
            {
                PropertyName = property.Name;
                DatabaseColumnName = property.GetColumnName();
                IsPrimaryKey = property.IsPrimaryKey();
                Value = property.GetValue(entity);
                PropertyDataType = property.PropertyType.Name.ToUpper();
                Generation = IsPrimaryKey ? property.GetGenerationOption() : property.GetCustomAttribute<DbGenerationOptionAttribute>() != null ? property.GetCustomAttribute<DbGenerationOptionAttribute>().Option : DbGenerationOption.None;

                // check for sql data translation, used mostly for datetime2 inserts and updates
                var translation = property.GetCustomAttribute<DbTypeAttribute>();

                if (translation != null)
                {
                    DbTranslationType = translation.Type;
                    TranslateDataType = true;
                }

                switch (Generation)
                {
                    case DbGenerationOption.None:
                        KeyName = "";
                        break;
                    case DbGenerationOption.IdentitySpecification:
                        KeyName = "@@IDENTITY";
                        break;
                    case DbGenerationOption.Generate:
                        KeyName = string.Format("@{0}", PropertyName);
                        // set as the property name so we can pull the value back out
                        break;
                }

                // for auto generation
                switch (property.PropertyType.Name.ToUpper())
                {
                    case "INT16":
                        SqlDataTypeString = "smallint";
                        break;
                    case "INT64":
                        SqlDataTypeString = "bigint";
                        break;
                    case "INT32":
                        SqlDataTypeString = "int";
                        break;
                    case "GUID":
                        SqlDataTypeString = "uniqueidentifier";
                        break;
                }
            }

            #endregion
        }

        private abstract class SqlModificationBuilder : SqlExecutionPlan, ISqlBuilder
        {
            protected SqlModificationBuilder(EntityInfo info) 
                : base(info)
            {
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
        }

        private class SqlExecutionPlan
        {
            public virtual List<ModifcationItem> GetModifcationItems()
            {
                return EntityInfo.GetAllColumns().Select(property => new ModifcationItem(property, Entity)).ToList();
            }

            public SqlExecutionPlan(EntityInfo info)
            {
                EntityInfo = info;
                _updateType = null;
            }

            private UpdateType? _updateType;

            // cache the update type
            public UpdateType UpdateType
            {
                get
                {
                    if (_updateType.HasValue) return _updateType.Value;

                    _updateType = EntityInfo.GetUpdateType();

                    return _updateType.Value;
                }
            }

            public readonly EntityInfo EntityInfo;
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
                FormattedTableName = plan.EntityInfo.SqlFormattedTableName();
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

        private abstract class SqlNonTransactionInsertBuilder : SqlModificationBuilder
        {
            protected SqlNonTransactionInsertBuilder(EntityInfo info)
                : base(info)
            {
            }

            public override ISqlPackage Build()
            {
                var package = new SqlNonTransactionInsertPackage(this);

                package.CreatePackage();

                return package;
            }
        }

        private abstract class SqlNonTransactionTryInsertBuilder : SqlNonTransactionInsertBuilder
        {
            protected SqlNonTransactionTryInsertBuilder(EntityInfo info) 
                : base(info)
            {
            }

            public override ISqlPackage Build()
            {
                var package = new SqlNonTransactionTryInsertPackage(this);

                package.CreatePackage();

                return package;
            }
        }

        private abstract class SqlNonTransactionTryInsertUpdateBuilder : SqlNonTransactionTryInsertBuilder
        {
            protected SqlNonTransactionTryInsertUpdateBuilder(EntityInfo info) 
                : base(info)
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

        private abstract class SqlNonTransactionUpdateBuilder : SqlNonTransactionTryInsertUpdateBuilder
        {
            protected SqlNonTransactionUpdateBuilder(EntityInfo info)
                : base(info)
            {
            }

            public override ISqlPackage Build()
            {
                var package = new SqlNonTransactionInsertPackage(this);

                package.CreatePackage();

                return package;
            }
        }

        #endregion

        #region Non Transaction Packages

        private class SqlNonTransactionUpdatePackage : SqlModificationPackage
        {
            #region Constructor

            public SqlNonTransactionUpdatePackage(SqlExecutionPlan builder) : base(builder)
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

        #endregion

        #endregion

        private abstract class SqlSkipModificationPackage : SqlNonTransactionUpdateBuilder
        {
            protected SqlSkipModificationPackage(EntityInfo info) 
                : base(info)
            {
            }
        }

        #region Unused

        private UpdateType _getState(object entity)
        {
            return _getState(entity, entity.GetPrimaryKeys());
        }

        private UpdateType _getState(object entity, List<PropertyInfo> primaryKeys)
        {
            var areAnyPkGenerationOptionsNone = false;

            var columns = entity.GetType().GetProperties().Where(w => !w.IsPrimaryKey()).ToList();

            var entityTrackable = entity as EntityStateTrackable;

            // make sure the user is not trying to update an IDENTITY column, these cannot be updated
            foreach (var column in
                columns.Where(w => w.GetCustomAttribute<DbGenerationOptionAttribute>() != null && w.GetCustomAttribute<DbGenerationOptionAttribute>().Option == DbGenerationOption.IdentitySpecification).Where(column => entityTrackable != null && EntityStateAnalyzer.HasColumnChanged(entityTrackable, column.Name)))
            {
                throw new SqlSaveException(string.Format("Cannot update value if IDENTITY column.  Column: {0}", column.Name));
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
                        throw new SqlSaveException(string.Format("Primary Key cannot be {1} for {2} when DbGenerationOption is set to None.  Primary Key Name: {0}", key.Name, pkValueTypeString, pkValueType));
                    }
                    continue;
                }

                // If we have only primary keys we need to perform a try insert and see if we can try to insert our data.
                // if we have any Pk's with a DbGenerationOption of None we need to first see if a record exists for the pks, 
                // if so we need to perform an update, otherwise we perform an insert
                return entity.HasPrimaryKeysOnly() ? UpdateType.TryInsert : areAnyPkGenerationOptionsNone ? UpdateType.TryInsertUpdate : UpdateType.Update;
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
            if (propertyType.IsGenericType && propertyType.GetGenericTypeDefinition() == typeof(Nullable<>))
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
            found.SetValue(entity, propertyType.IsEnum ? Enum.ToObject(propertyType, value) : Convert.ChangeType(value, propertyType), null);
        }

        private void _setPropertyValue(object entity, PropertyInfo property, object value)
        {
            var propertyType = property.PropertyType;

            //Nullable properties have to be treated differently, since we 
            //  use their underlying property to set the value in the object
            if (propertyType.IsGenericType && propertyType.GetGenericTypeDefinition() == typeof(Nullable<>))
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
            property.SetValue(entity, propertyType.IsEnum ? Enum.ToObject(propertyType, value) : Convert.ChangeType(value, propertyType), null);
        }

        private void _setPropertyValue(object parent, object child, string propertyNameToSet)
        {
            if (parent == null) return;

            var foreignKeyProperty = parent.GetForeignKeys().First(w => (w.PropertyType.IsList() ? w.PropertyType.GetGenericArguments()[0] : w.PropertyType) == child.GetType() && w.Name == propertyNameToSet);

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

        private void _getSaveOrder<T>(T entity, List<ForeignKeySaveNode> savableObjects, bool isMARSEnabled) where T : class
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
                if (tableInfo.IsReadOnly && tableInfo.GetReadOnlySaveOption() == ReadOnlySaveOption.Skip)
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
                        throw new SqlSaveException(string.Format("Foreign Key Has No Value - Foreign Key Property Name: {0}.  If the ForeignKey is nullable, make the ID nullable in the POCO to save", e.GetType().Name));
                    }
                }

                // Check for readonly attribute and see if we should throw an error
                if (tableInfo.IsReadOnly && tableInfo.GetReadOnlySaveOption() == ReadOnlySaveOption.ThrowException)
                {
                    throw new SqlSaveException(string.Format("Table Is ReadOnly.  Table: {0}.  Change ReadOnlySaveOption to Skip if you wish to skip this table and its foreign keys", entity.GetTableName()));
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
            return entity.GetForeignKeys().OrderBy(w => w.PropertyType.IsList()).Select(w => new ParentChildPair(entity, w.GetValue(entity), w)).ToList();
        }

        #region helpers

        private class ParentChildPair
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

        #endregion
    }
}
