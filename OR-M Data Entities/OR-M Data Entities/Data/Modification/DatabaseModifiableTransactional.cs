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
using OR_M_Data_Entities.Configuration;
using OR_M_Data_Entities.Data.Definition;
using OR_M_Data_Entities.Data.Modification;
using OR_M_Data_Entities.Data.Query;
using OR_M_Data_Entities.Data.Secure;
using OR_M_Data_Entities.Exceptions;

// ReSharper disable once CheckNamespace
namespace OR_M_Data_Entities.Data
{
    public partial class DatabaseModifiable
    {
        #region Sql Container

        private class SqlTransactionContainer : ISqlContainer
        {
            private readonly string _sql;

            private readonly string _declare;

            private readonly string _set;

            public SqlTransactionContainer(string sql, string declare = null, string set = null)
            {
                _sql = sql;
                _declare = declare;
                _set = set;
            }

            public string Resolve()
            {
                return Split().ToString();
            }

            public SqlPartStatement Split()
            {
                return new SqlPartStatement(_sql, _declare, _set);
            }
        }

        private class TransactionUpdateContainer : UpdateContainer
        {
            private string _tableVariables { get; set; }

            private readonly string _tableAlias;

            private const string SQL_CONCURRENCY_VIOLATION_MESSAGE = "Sql Concurrency Violation on table [{0}]";

            private const int SQL_CONCURRENCY_VIOLATION_ERROR_STATE = 1122;

            private const int SQL_CONCURRENCY_VIOLATION_ERROR_SEVERITY = 18;

            private readonly IConfigurationOptions _configuration;

            public TransactionUpdateContainer(IModificationEntity entity, IConfigurationOptions configuration, string tableAlias)
                : base(entity)
            {
                _tableAlias = tableAlias;
                _configuration = configuration;
            }

            public override SqlPartStatement Split()
            {
                Output = string.Concat(Output, string.Format(" INTO @{0}", _tableAlias));
                //
                // need output so we can see how many rows were updated.  Needed for concurrency checking
                var sql = string.Format("{0} SET {1} OUTPUT {2} WHERE {3};",

                    Statement,

                    SetItems.TrimEnd(','),

                    Output,

                    Where.TrimEnd(','));

                if (_configuration.ConcurrencyChecking.IsOn)
                {
                    if (_configuration.ConcurrencyChecking.ViolationRule == ConcurrencyViolationRule.ThrowException)
                    {
                        var errorMessage = string.Format(SQL_CONCURRENCY_VIOLATION_MESSAGE, SqlFormattedTableName);

                        sql = string.Concat(sql,
                            string.Format(
                                "\rIF (NOT(EXISTS(SELECT TOP 1 1 FROM @{0})))\r\tBEGIN\r\t\tRAISERROR('{1}', {2}, {3})\r\tEND",

                                _tableAlias,

                                errorMessage,

                                SQL_CONCURRENCY_VIOLATION_ERROR_SEVERITY,

                                SQL_CONCURRENCY_VIOLATION_ERROR_STATE));
                    }
                    else
                    {
                        // just need to make sure rows exists for concurrency checking
                        sql = string.Concat(sql, string.Format("\rSELECT TOP 1 1 FROM @{0}", _tableAlias));
                    }
                }

                var declare = string.Format("DECLARE @{0} TABLE({1});\r", _tableAlias, _tableVariables.TrimEnd(','));

                return new SqlPartStatement(sql, declare);
            }

            public void AddTableVariable(IModificationItem item)
            {
                _tableVariables = string.Concat(_tableVariables, string.Format("{0} {1},", item.PropertyName, item.SqlDataTypeString));
            }
        }

        private class TransactionDeleteContainer : DeleteContainer
        {
            private string _tableVariables { get; set; }

            private readonly string _tableAlias;

            public TransactionDeleteContainer(IModificationItem entity, string tableAlias)
                : base(entity)
            {
                _tableAlias = tableAlias;
            }

            public void AddTableVariable(IModificationItem item)
            {
                _tableVariables = string.Concat(_tableVariables, string.Format("{0} {1},", item.PropertyName, item.SqlDataTypeString));
            }

            public override SqlPartStatement Split()
            {
                var sql = string.Format("{0} {1} WHERE {2}", Statement, Output, Where.TrimEnd(','));

                return new SqlPartStatement(sql);
            }
        }

        private class TransactionInsertContainer : InsertContainer
        {
            private string _tableVariables { get; set; }

            private readonly string _tableAlias;

            public TransactionInsertContainer(IModificationEntity entity, string tableAlias)
                : base(entity)
            {
                _tableAlias = tableAlias;
            }

            public void AddTableVariable(IModificationItem item)
            {
                _tableVariables = string.Concat(_tableVariables, string.Format("{0} {1},", item.PropertyName, item.SqlDataTypeString));
            }

            public override SqlPartStatement Split()
            {
                // try insert does not need to select anything back, either it succeeded or didnt.
                var selectBackStatement = !string.IsNullOrEmpty(_outputColumnsOnly)
                    ? string.Format("\rSELECT {0} FROM @{1}", _outputColumnsOnly.TrimEnd(','), _tableAlias)
                    : string.Empty;

                var outputStatement = !string.IsNullOrEmpty(_output)
                    ? string.Format(" OUTPUT {0} INTO @{1}", _output.TrimEnd(','), _tableAlias)
                    : string.Empty;

                var sql = string.Format("INSERT INTO [{0}] ({1}){2} VALUES ({3});{4}",

                    SqlFormattedTableName,

                    _fields.TrimEnd(','),

                    outputStatement,

                    _values.TrimEnd(','),

                    selectBackStatement

                    // we want to select everything back from the database in case the model relies on db generation for some fields.
                    // this way we can load the data back into the model.  Works perfect for things like time stamps and auto generation
                    // where the column is not the PK
                    );

                if (!string.IsNullOrEmpty(_tableVariables))
                {
                    _declare = string.Concat(_declare, string.Format("DECLARE @{0} TABLE({1});\r", _tableAlias, _tableVariables.TrimEnd(',')));
                }

                return new SqlPartStatement(sql, _declare, _set);
            }
        }

        #endregion

        #region Plans

        private class SqlTransactionPlan : ISqlExecutionPlan
        {
            private readonly ReferenceMap _referenceMap;

            private readonly List<ISqlExecutionPlan> _builders;

            private readonly List<SqlSecureQueryParameter> _parameters;

            public SqlTransactionPlan(ReferenceMap map, List<SqlSecureQueryParameter> parameters)
            {
                _referenceMap = map;
                _builders = new List<ISqlExecutionPlan>();
                _parameters = parameters;
            }

            public void Add<T>(T builder) where T : ISqlExecutionPlan, ISqlTransaction
            {
                _builders.Add(builder);
            }

            public ISqlBuilder GetBuilder()
            {
                if (_builders == null || _builders.Count == 0)
                {
                    throw new SqlSaveException("No items to save");
                }

                return new SqlTransactionBuilder(_referenceMap, _builders, _parameters);
            }

            public SqlCommand BuildSqlCommand(SqlConnection connection)
            {
                // get the builder
                var package = GetBuilder();

                // generate the sql command
                var command = new SqlCommand(package.GetSql(), connection);

                // insert the parameters
                package.InsertParameters(command);

                return command;
            }

            public IModificationEntity Entity { get; private set; }
        }

        private class SqlTransactionInsertPlan : SqlInsertPlan, ISqlTransaction
        {
            public Reference Reference { get; private set; }

            public SqlTransactionInsertPlan(ModificationEntity entity, List<SqlSecureQueryParameter> sharedParameters, Reference reference, IConfigurationOptions configuration)
                : base(entity, configuration, sharedParameters)
            {
                Reference = reference;
            }

            public override ISqlBuilder GetBuilder()
            {
                return new SqlTransactionInsertBuilder(this, Parameters, Reference, Configuration);
            }
        }

        private class SqlTransactionTryInsertPlan : SqlTryInsertPlan, ISqlTransaction
        {
            public Reference Reference { get; private set; }

            public SqlTransactionTryInsertPlan(ModificationEntity entity, List<SqlSecureQueryParameter> sharedParameters, Reference reference, IConfigurationOptions configuration)
                : base(entity, configuration, sharedParameters)
            {
                Reference = reference;
            }

            public override ISqlBuilder GetBuilder()
            {
                var insert = new SqlTransactionInsertBuilder(this, Parameters, Reference, Configuration);

                return new SqlExistsBuilder(this, Configuration, Parameters, insert);
            }
        }

        private class SqlTransactionTryInsertUpdatePlan : SqlTryInsertUpdatePlan, ISqlTransaction
        {
            public Reference Reference { get; private set; }

            public SqlTransactionTryInsertUpdatePlan(ModificationEntity entity, List<SqlSecureQueryParameter> parameters, Reference reference, IConfigurationOptions configuration)
                : base(entity, configuration, parameters)
            {
                Reference = reference;
            }

            public override ISqlBuilder GetBuilder()
            {
                // insert and update need to share (by reference) their parameters list so they are in sync and do not overlap keys
                var insert = new SqlTransactionInsertBuilder(this, Parameters, Reference, Configuration);

                // change table alias otherwise they will be the same and we will get errors
                var update = new SqlTransactionUpdateBuilder(this, Parameters, Reference, Configuration, string.Format("{0}_1", Reference.Alias));

                return new SqlExistsBuilder(this, Configuration, Parameters, insert, update);
            }
        }

        private class SqlTransactionUpdatePlan : SqlUpdatePlan, ISqlTransaction
        {
            public Reference Reference { get; private set; }

            public SqlTransactionUpdatePlan(ModificationEntity entity, List<SqlSecureQueryParameter> parameters, Reference reference, IConfigurationOptions configuration)
                : base(entity, configuration, parameters)
            {
                Reference = reference;
            }

            public override ISqlBuilder GetBuilder()
            {
                return new SqlTransactionUpdateBuilder(this, Parameters, Reference, Configuration);
            }
        }

        private class SqlTransactionDeletePlan : SqlDeletePlan, ISqlTransaction
        {
            public Reference Reference { get; private set; }

            public SqlTransactionDeletePlan(ModificationEntity entity, List<SqlSecureQueryParameter> parameters, Reference reference, IConfigurationOptions configuration) 
                : base(entity, configuration, parameters)
            {
                Reference = reference;
            }

            public override ISqlBuilder GetBuilder()
            {
                return new SqlDeleteBuilder(this, Configuration, Parameters);
            }
        }

        private interface ISqlTransaction
        {
            Reference Reference { get; }
        }

        #endregion

        #region Package

        private class SqlTransactionBuilder : ISqlBuilder
        {
            private readonly List<ISqlExecutionPlan> _builders;

            private readonly ReferenceMap _referenceMap;

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

            private readonly List<SqlSecureQueryParameter> _parameters;

            public SqlTransactionBuilder(ReferenceMap referenceMap, List<ISqlExecutionPlan> builders, List<SqlSecureQueryParameter> parameters)
            {
                _builders = builders;
                _referenceMap = referenceMap;
                _parameters = parameters;
            }

            public string GetSql()
            {
                var container = BuildContainer();

                return container.Resolve();
            }

            public ISqlContainer BuildContainer()
            {
                var sql = string.Empty;
                var declare = string.Empty;
                var set = string.Empty;

                // create our transaction package here
                for (var i = 0; i < _builders.Count; i++)
                {
                    var builder = _builders[i];

                    // build the package
                    var package = builder.GetBuilder();

                    // create the container
                    var container = package.BuildContainer();

                    // split the sql into the set, sql, and declare statements
                    var split = container.Split();

                    sql = string.Concat(sql, string.Format("{0};\r", split.Sql));

                    if (!string.IsNullOrEmpty(split.Declare))
                    {
                        declare = string.Concat(declare, split.Declare);
                    }

                    if (!string.IsNullOrEmpty(split.Set))
                    {
                        set = string.Concat(set, split.Set);
                    }
                }

                sql = string.Format(_transactionSqlBase, sql);

                return new SqlTransactionContainer(sql, declare, set);
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
        }

        private class SqlTransactionDeleteBuilder : SqlDeleteBuilder
        {
            private readonly Reference _reference;

            public SqlTransactionDeleteBuilder(ISqlExecutionPlan builder, List<SqlSecureQueryParameter> parameters, Reference reference, IConfigurationOptions configuration)
                : base(builder, configuration, parameters)
            {
                _reference = reference;
            }

            protected override void AddWhere(ISqlContainer container, IModificationItem item)
            {
                ((TransactionDeleteContainer)container).AddTableVariable(item);

                base.AddWhere(container, item);
            }

            protected override ISqlContainer NewContainer()
            {
                return new DeleteContainer(Entity);
            }
        }

        private class SqlTransactionInsertBuilder : SqlInsertBuilder
        {
            private readonly Reference _reference;

            private readonly string _tableAliasOverride;

            public SqlTransactionInsertBuilder(ISqlExecutionPlan builder, List<SqlSecureQueryParameter> parameters, Reference reference, IConfigurationOptions configuration, string tableAliasOverride = null)
                : base(builder, configuration, parameters)
            {
                _reference = reference;
                _tableAliasOverride = tableAliasOverride;
            }

            private ReferenceNode _getReferenceNode(IModificationItem item)
            {
                return _reference.References.FirstOrDefault(w => w.Link.ParentColumnName == item.DatabaseColumnName);
            }

            protected override ISqlContainer NewContainer()
            {
                return new TransactionInsertContainer(Entity,
                    string.IsNullOrEmpty(_tableAliasOverride) ? _reference.Alias : _tableAliasOverride);
            }

            protected override void AddDbGenerationOptionNone(ISqlContainer container, IModificationItem item)
            {
                if (item.DbTranslationType == SqlDbType.Timestamp)
                {
                    ((TransactionInsertContainer)container).AddOutput(item);
                    return;
                }

                ((TransactionInsertContainer)container).AddField(item);

                // reference comes from a different table, should never have any generation options
                var node = _getReferenceNode(item);

                if (node != null)
                {
                    // grab the reference from the other table
                    ((TransactionInsertContainer)container).AddValue(node.GetOutputFieldValue());
                    return;
                }

                // parameter key
                var value = Entity.GetPropertyValue(item.PropertyName);
                var data = AddParameter(item, value);

                ((TransactionInsertContainer)container).AddValue(data);
            }

            protected override void AddDbGenerationOptionGenerate(ISqlContainer container, IModificationItem item)
            {
                ((TransactionInsertContainer)container).AddTableVariable(item);

                base.AddDbGenerationOptionGenerate(container, item);
            }

            protected override void AddDbGenerationOptionIdentityAndDefault(ISqlContainer container, IModificationItem item)
            {
                ((TransactionInsertContainer)container).AddTableVariable(item);

                base.AddDbGenerationOptionIdentityAndDefault(container, item);
            }
        }

        private class SqlTransactionUpdateBuilder : SqlUpdateBuilder
        {
            private readonly Reference _reference;


            private readonly string _tableAliasOverride;

            public SqlTransactionUpdateBuilder(ISqlExecutionPlan builder, List<SqlSecureQueryParameter> parameters, Reference reference, IConfigurationOptions configuration, string tableAliasOverride = null)
                : base(builder, configuration, parameters)
            {
                _reference = reference;
                _tableAliasOverride = tableAliasOverride;
            }

            protected override ISqlContainer NewContainer()
            {
                return new TransactionUpdateContainer(Entity, Configuration,
                    string.IsNullOrEmpty(_tableAliasOverride) ? _reference.Alias : _tableAliasOverride);
            }

            protected override void AddWhere(ISqlContainer container, IModificationItem item)
            {
                // add the table variable, needed for concurrency checking
                if (item.IsPrimaryKey) ((TransactionUpdateContainer)container).AddTableVariable(item);

                base.AddWhere(container, item);
            }
        }

        #endregion

        #region Methods

        private int _getNextReferenceIndex(int currentIndex, IReadOnlyList<UpdateType> saves)
        {
            for (var i = 0; i < saves.Count; i++)
            {
                var save = saves[i];

                if (i > currentIndex && save != UpdateType.Skip) return i;
            }

            return -1;
        }

        private List<UpdateType> _getActionsTaken(ReferenceMap referenceMap)
        {
            var currentSaveIndex = -1;// nothing
            var concurrencyViolations = new List<object>();
            var index = 0;
            var result = new List<UpdateType>();

            // loads the data that was deleted
            do
            {
                // reference map and result sets are in the same order
                var reference = referenceMap[index];

                if (!Reader.HasRows)
                {
                    // check to see if there was a concurrency violation.  Other enum cases will be handled in different area
                    if (reference.Entity.UpdateType == UpdateType.Update &&
                        Configuration.ConcurrencyChecking.IsOn &&
                        Configuration.ConcurrencyChecking.ViolationRule == ConcurrencyViolationRule.UseHandler)
                    {
                        // connection is still open here, handle the concurrency violation later
                        // if there is a concurrency violation do not load anytihng
                        concurrencyViolations.Add(reference.Entity.Value);
                    }

                    // no rows?  Continue
                    index++;
                    result.Add(UpdateType.RowNotFound);
                    continue;
                }

                index++;
                result.Add(UpdateType.TransactionalDelete);
            } while (Reader.NextResult()); // go to next result

            // close reader and connection
            Disconnect();

            return result;
        } 

        private void _loadObjectFromMultipleActiveResults(ReferenceMap referenceMap, IReadOnlyList<UpdateType> saves)
        {
            var currentSaveIndex = -1;// nothing
            var concurrencyViolations = new List<object>();

            // loads the data back into the object from the MARS data set
            do
            {
                // get the save index 
                currentSaveIndex = _getNextReferenceIndex(currentSaveIndex, saves);

                // reference map and result sets are in the same order
                var reference = referenceMap[currentSaveIndex];

                if (!Reader.HasRows)
                {
                    // check to see if there was a concurrency violation.  Other enum cases will be handled in different area
                    if (reference.Entity.UpdateType == UpdateType.Update &&
                        Configuration.ConcurrencyChecking.IsOn &&
                        Configuration.ConcurrencyChecking.ViolationRule == ConcurrencyViolationRule.UseHandler)
                    {
                        // connection is still open here, handle the concurrency violation later
                        // if there is a concurrency violation do not load anytihng
                        concurrencyViolations.Add(reference.Entity.Value);
                    }

                    // no rows?  Continue
                    continue;
                }

                // Process all elements in the current result set
                while (Reader.Read())
                {
                    var keyContainer = new OutputContainer();
                    var rec = (IDataRecord)Reader;

                    for (var j = 0; j < rec.FieldCount; j++)
                    {
                        keyContainer.Add(rec.GetName(j), rec.GetValue(j));

                        reference.Entity.SetPropertyValue(rec.GetName(j), rec.GetValue(j));

                        // set any fk's
                        if (reference.Parent == null) continue;

                        // set property will check the relationship and set appropiately
                        Entity.SetPropertyValue(reference.Parent, reference.Entity.Value, reference.Property.Name);
                    }
                }

            } while (Reader.NextResult()); // go to next result

            // close reader and connection
            Disconnect();

            // handle concurrency violations
            if (OnConcurrencyViolation == null) return;

            // handle here so the connection is not left open
            foreach (var violation in concurrencyViolations)
            {
                OnConcurrencyViolation(violation);
            }
        }

        private bool _saveChangesUsingTransactions<T>(T entity) where T : class
        {
            var saves = new List<UpdateType>();
            var parent = new ModificationEntity(entity, Configuration);

            // get all items to save and get them in order
            var referenceMap = EntityMapper.GetReferenceMap(parent, Configuration);
            var parameters = new List<SqlSecureQueryParameter>();
            var plan = new SqlTransactionPlan(referenceMap, parameters);

            for (var i = 0; i < referenceMap.Count; i++)
            {
                var reference = referenceMap[i];

                // add the save to the list so we can tell the user what the save action did
                saves.Add(reference.Entity.UpdateType);

                if (OnBeforeSave != null) OnBeforeSave(reference.Entity.Value, reference.Entity.UpdateType);

                // Get the correct execution plan
                switch (reference.Entity.UpdateType)
                {
                    case UpdateType.Insert:
                        plan.Add(new SqlTransactionInsertPlan(reference.Entity, parameters, reference, Configuration));
                        break;
                    case UpdateType.TryInsert:
                        plan.Add(new SqlTransactionTryInsertPlan(reference.Entity, parameters, reference, Configuration));
                        break;
                    case UpdateType.TryInsertUpdate:
                        plan.Add(new SqlTransactionTryInsertUpdatePlan(reference.Entity, parameters, reference, Configuration));
                        break;
                    case UpdateType.Update:
                        plan.Add(new SqlTransactionUpdatePlan(reference.Entity, parameters, reference, Configuration));
                        break;
                    case UpdateType.Skip:
                        continue;
                    default:
                        throw new ArgumentOutOfRangeException();
                }
            }

            if (OnSaving != null) OnSaving(entity);

            // execute the sql.  make sure all the saves are not skips
            if (!(saves.All(w => w == UpdateType.Skip)))
            {
                ExecuteReader(plan);

                // load back the data from the save into the model
                _loadObjectFromMultipleActiveResults(referenceMap, saves);

                // set the pristine state of each entity.  Cannot set above because the object will not save correctly.
                // Object needs to be loaded with the data from the database first
                for (var i = 0; i < referenceMap.Count; i++)
                {
                    var reference = referenceMap[i];

                    // set the pristine state only if entity tracking is on
                    if (reference.Entity.IsEntityStateTrackingOn) ModificationEntity.TrySetPristineEntity(reference.Entity.Value);
                }
            }

            if (OnAfterSave != null) OnAfterSave(entity, UpdateType.TransactionalSave);

            return saves.Any(w => w != UpdateType.Skip);
        }

        private bool _deleteUsingTransactions<T>(T entity)
            where T : class
        {
            var parent = new DeleteEntity(entity, Configuration);

            // get all items to save and get them in order
            var referenceMap = EntityMapper.GetReferenceMap(parent, Configuration);
            var parameters = new List<SqlSecureQueryParameter>();
            var builder = new SqlTransactionPlan(referenceMap, parameters);

            // reverse the order to back them out of the database
            referenceMap.Reverse();

            for (var i = 0; i < referenceMap.Count; i++)
            {
                var reference = referenceMap[i];

                if (OnBeforeSave != null) OnBeforeSave(reference.Entity.Value, reference.Entity.UpdateType);

                builder.Add(new SqlTransactionDeletePlan(reference.Entity, parameters, reference, Configuration));
            }

            if (OnSaving != null) OnSaving(entity);

            // execute the sql
            ExecuteReader(builder);

            // check to see if the rows were deleted or not
            var saves = _getActionsTaken(referenceMap);

            // set the pristine state of each entity.  Cannot set above because the object will not save correctly.
            // Object needs to be loaded with the data from the database first
            for (var i = 0; i < referenceMap.Count; i++)
            {
                var reference = referenceMap[i];

                // set the pristine state only if entity tracking is on
                if (reference.Entity.IsEntityStateTrackingOn) ModificationEntity.TrySetPristineEntity(reference.Entity.Value);
            }

            if (OnAfterSave != null) OnAfterSave(entity, UpdateType.TransactionalSave);

            return saves.Any(w => w == UpdateType.TransactionalDelete);
        }

        #endregion
    }
}
