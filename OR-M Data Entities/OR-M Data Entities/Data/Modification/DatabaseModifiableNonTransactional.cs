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
using OR_M_Data_Entities.Configuration;
using OR_M_Data_Entities.Data.Definition;
using OR_M_Data_Entities.Data.Modification;
using OR_M_Data_Entities.Data.Query;
using OR_M_Data_Entities.Data.Secure;
using OR_M_Data_Entities.Exceptions;
using OR_M_Data_Entities.Mapping;

// ReSharper disable once CheckNamespace
namespace OR_M_Data_Entities.Data
{
    /// <summary>
    /// Created partial class to split off query builders.  
    /// We do not want the end user to know about any sql builders, we hide them all in the partial class
    /// </summary>
    public partial class DatabaseModifiable
    {
        #region Sql Containers
        private class CustomContainer : SqlModificationContainer, ISqlContainer
        {
            private readonly string _sql;

            public CustomContainer(IModificationEntity entity, string sql)
                : base(entity)
            {
                _sql = sql;
            }

            public string Resolve()
            {
                return Split().ToString();
            }

            public ISqlPartStatement Split()
            {
                return new SqlPartStatement(_sql);
            }
        }

        private class InsertContainer : SqlModificationContainer, ISqlContainer
        {
            #region Properties
            protected string _fields { get; private set; }

            protected string _values { get; private set; }

            protected string _declare { get; set; }

            protected string _output { get; private set; }

            protected string _outputColumnsOnly { get; private set; }

            protected string _set { get; set; }
            #endregion

            #region Constructor
            public InsertContainer(IModificationEntity entity)
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
            public void AddField(IModificationItem item)
            {
                _fields = string.Concat(_fields, item.AsField(","));
            }

            public void AddValue(string parameterKey)
            {
                _values = string.Concat(_values, string.Format("{0},", parameterKey));
            }

            public void AddDeclare(string parameterKey, string sqlDataType)
            {
                _declare = string.Concat(_declare, string.Format("DECLARE {0} as {1};\r", parameterKey, sqlDataType));
            }

            public void AddOutput(IModificationItem item)
            {
                _output = string.Concat(_output, item.AsOutput(","));
                _outputColumnsOnly = string.Concat(_outputColumnsOnly, item.AsFieldPropertyName(","));
            }

            public void AddSetGenerationItem(IModificationItem item, out string key)
            {
                // make sure we can generate the key they are specifying
                key = string.Format("@{0}", item.GetUniqueKey());

                switch (item.DbTranslationType)
                {
                    case SqlDbType.SmallMoney:
                    case SqlDbType.TinyInt:
                    case SqlDbType.SmallInt:
                    case SqlDbType.Real:
                    case SqlDbType.Money:
                    case SqlDbType.Int:
                    case SqlDbType.BigInt:
                    case SqlDbType.Float:
                    case SqlDbType.Decimal:
                        _set = string.Concat(_set, string.Format("SET {0} = (Select ISNULL(MAX([{1}]),0) + 1 From [{2}]);\r", key, item.DatabaseColumnName, SqlFormattedTableName));
                        break;

                    case SqlDbType.Time:
                    case SqlDbType.Date:
                    case SqlDbType.VarBinary:
                    case SqlDbType.VarChar:
                    case SqlDbType.Variant:
                    case SqlDbType.Xml:
                    case SqlDbType.Udt:
                    case SqlDbType.Text:
                    case SqlDbType.Timestamp:
                    case SqlDbType.SmallDateTime:
                    case SqlDbType.Structured:
                    case SqlDbType.NChar:
                    case SqlDbType.NText:
                    case SqlDbType.NVarChar:
                    case SqlDbType.Image:
                    case SqlDbType.Char:
                    case SqlDbType.Bit:
                    case SqlDbType.Binary:
                        throw new Exception(string.Format("Cannot generate Primary Key for type of {0}. Column Name - {1}", item.DbTranslationType, item.PropertyName));

                    case SqlDbType.DateTime:
                        break;
                    
                    case SqlDbType.UniqueIdentifier:
                        _set = string.Concat(_set, string.Format("SET {0} = NEWID();\r", key));
                        break;

                    case SqlDbType.DateTime2:
                        _set = string.Concat(_set, string.Format("SET {0} = '{1}';\r", key, DateTime.Now));
                        break;

                    case SqlDbType.DateTimeOffset:
                        _set = string.Concat(_set, string.Format("SET {0} = '{1}';\r", key, DateTimeOffset.Now));
                        break;

                    default:
                        throw new ArgumentOutOfRangeException();
                }
            }

            public string Resolve()
            {
                return Split().ToString();
            }

            public virtual ISqlPartStatement Split()
            {
                var sql = string.Format("INSERT INTO [{0}] ({1}) OUTPUT {2} VALUES ({3})", SqlFormattedTableName, _fields.TrimEnd(','), _output.TrimEnd(','), _values.TrimEnd(',')

                    // we want to select everything back from the database in case the model relies on db generation for some fields.
                    // this way we can load the data back into the model.  Works perfect for things like time stamps and auto generation
                    // where the column is not the PK
                    );

                return new SqlPartStatement(sql, _declare, _set);
            }

            #endregion
        }

        private class UpdateContainer : SqlModificationContainer, ISqlContainer
        {
            protected string SetItems { get; set; }

            protected string Where { get; set; }

            protected readonly string Statement;

            protected string Output { get; set; }

            public UpdateContainer(IModificationEntity entity) : base(entity)
            {
                SetItems = string.Empty;
                Where = string.Empty;
                Statement = string.Format("UPDATE [{0}]", SqlFormattedTableName);

                var keys = entity.GetPrimaryKeys();

                if (keys.Count == 0) throw new KeyNotFoundException(string.Format("Primary Key not found for table: {0}", entity.PlainTableName));
            }

            public void AddOutput(IModificationItem item)
            {
                Output = string.Concat(Output, string.Format("[INSERTED].[{0}],", item.DatabaseColumnName));
            }

            public void AddUpdate(IModificationItem item, string parameterKey)
            {
                SetItems = string.Concat(SetItems, string.Format("[{0}] = {1},", item.DatabaseColumnName, parameterKey));
            }

            public void AddWhere(IModificationItem item, string parameterKey)
            {
                Where = string.Concat(Where, string.Format("{0}[{1}] = {2}", _getWherePrefix(), item.DatabaseColumnName, parameterKey));
            }

            public void AddNullWhere(IModificationItem item)
            {
                Where = string.Concat(Where, string.Format("{0}[{1}] IS NULL", _getWherePrefix(), item.DatabaseColumnName));
            }

            private string _getWherePrefix()
            {
                return string.IsNullOrEmpty(Where) ? string.Empty : " AND ";
            }

            public string Resolve()
            {
                return Split().ToString();
            }

            public virtual ISqlPartStatement Split()
            {
                // need output so we can see how many rows were updated.  Needed for concurrency checking
                var sql = string.Format("{0} SET {1} OUTPUT {2} WHERE {3}", Statement, SetItems.TrimEnd(','), Output.TrimEnd(','), Where.TrimEnd(','));

                return new SqlPartStatement(sql);
            }
        }

        private class DeleteContainer : SqlModificationContainer, ISqlContainer
        {
            protected string Where { get; set; }

            protected string Output { get; set; }

            protected readonly string Statement;

            public DeleteContainer(IModificationEntity entity) : base(entity)
            {
                Output = entity.GetPrimaryKeys().Select(Table.GetColumnName).Aggregate("OUTPUT ", (s, s1) => string.Concat(s, string.Format("[DELETED].[{0}],", s1))).TrimEnd(',');

                Where = string.Empty;
                Statement = string.Format("DELETE FROM [{0}]", SqlFormattedTableName);
            }

            public void AddWhere(IModificationItem item, string parameterKey)
            {
                Where = string.Concat(Where, string.Format("{0}[{1}] = {2}", _getWherePrefix(), item.DatabaseColumnName, parameterKey));
            }

            public void AddNullWhere(IModificationItem item)
            {
                Where = string.Concat(Where, string.Format("{0}[{1}] IS NULL", _getWherePrefix(), item.DatabaseColumnName));
            }

            private string _getWherePrefix()
            {
                return string.IsNullOrEmpty(Where) ? string.Empty : " AND ";
            }

            public string Resolve()
            {
                return Split().ToString();
            }

            public virtual ISqlPartStatement Split()
            {
                var sql = string.Format("{0} {1} WHERE {2}", Statement, Output, Where.TrimEnd(','));

                return new SqlPartStatement(sql);
            }
        }

        protected abstract class SqlModificationContainer
        {
            protected SqlModificationContainer(IModificationEntity entity)
            {
                SqlFormattedTableName = entity.ToString(TableNameFormat.SqlWithSchemaTrimStartAndEnd);
            }

            protected readonly string SqlFormattedTableName;

            protected string InsertTableVariable(IModificationItem item, string tableVariables)
            {
                // cannot insert a timestamp into a table variable.  A timestamp is a binary.  Convert the timestamp to a 
                // varbinary so we can insert it into the table variable and read it back
                var sqlDataTypeString = item.DbTranslationType == SqlDbType.Timestamp ? string.Format("{0}(MAX)", SqlDbType.VarBinary) : item.SqlDataTypeString;

                return string.Concat(tableVariables, string.Format("{0} {1},", item.PropertyName, sqlDataTypeString));
            }
        }

        #endregion

        #region Plans

        private class SqlInsertPlan : SqlExecutionPlan
        {
            public SqlInsertPlan(ModificationEntity entity, IConfigurationOptions configurationOptions) : base(entity, configurationOptions, new List<SqlSecureQueryParameter>())
            {
            }

            protected SqlInsertPlan(ModificationEntity entity, IConfigurationOptions configurationOptions, List<SqlSecureQueryParameter> sharedParameters) : base(entity, configurationOptions, sharedParameters)
            {
            }

            public override ISqlBuilder GetBuilder()
            {
                return new SqlInsertBuilder(this, Configuration, Parameters);
            }
        }

        private class SqlTryInsertPlan : SqlExecutionPlan
        {
            public SqlTryInsertPlan(ModificationEntity entity, IConfigurationOptions configurationOptions) : base(entity, configurationOptions, new List<SqlSecureQueryParameter>())
            {
            }

            protected SqlTryInsertPlan(ModificationEntity entity, IConfigurationOptions configurationOptions, List<SqlSecureQueryParameter> sharedParameters) : base(entity, configurationOptions, sharedParameters)
            {
            }

            public override ISqlBuilder GetBuilder()
            {
                var insert = new SqlInsertBuilder(this, Configuration, Parameters);

                return new SqlExistsBuilder(this, Configuration, Parameters, insert);
            }
        }

        private class SqlTryInsertUpdatePlan : SqlExecutionPlan
        {
            public SqlTryInsertUpdatePlan(ModificationEntity entity, IConfigurationOptions configurationOptions) : base(entity, configurationOptions, new List<SqlSecureQueryParameter>())
            {
            }

            protected SqlTryInsertUpdatePlan(ModificationEntity entity, IConfigurationOptions configurationOptions, List<SqlSecureQueryParameter> sharedParameters) : base(entity, configurationOptions, sharedParameters)
            {
            }

            public override ISqlBuilder GetBuilder()
            {
                // insert and update need to share (by reference) their parameters list so they are in sync and do not overlap keys
                var parameters = new List<SqlSecureQueryParameter>();
                var insert = new SqlInsertBuilder(this, Configuration, parameters);
                var update = new SqlUpdateBuilder(this, Configuration, parameters);

                return new SqlExistsBuilder(this, Configuration, parameters, insert, update);
            }
        }

        private class SqlUpdatePlan : SqlExecutionPlan
        {
            public SqlUpdatePlan(ModificationEntity entity, IConfigurationOptions configurationOptions) : base(entity, configurationOptions, new List<SqlSecureQueryParameter>())
            {
            }

            protected SqlUpdatePlan(ModificationEntity entity, IConfigurationOptions configurationOptions, List<SqlSecureQueryParameter> sharedParameters) : base(entity, configurationOptions, sharedParameters)
            {
            }

            public override ISqlBuilder GetBuilder()
            {
                return new SqlUpdateBuilder(this, Configuration, Parameters);
            }
        }

        private class SqlDeletePlan : SqlExecutionPlan
        {
            public SqlDeletePlan(ModificationEntity entity, IConfigurationOptions configurationOptions) : base(entity, configurationOptions, new List<SqlSecureQueryParameter>())
            {
            }

            protected SqlDeletePlan(ModificationEntity entity, IConfigurationOptions configurationOptions, List<SqlSecureQueryParameter> sharedParameters) : base(entity, configurationOptions, sharedParameters)
            {
            }

            public override ISqlBuilder GetBuilder()
            {
                return new SqlDeleteBuilder(this, Configuration, Parameters);
            }
        }

        #endregion

        #region Builders

        private class SqlExistsBuilder : SqlModificationBuilder
        {
            private SqlModificationBuilder _exists { get; set; }

            private SqlModificationBuilder _notExists { get; set; }

            private string _existsStatement { get; set; }

            private string _where { get; set; }

            public SqlExistsBuilder(ISqlExecutionPlan plan, IConfigurationOptions configurationOptions, List<SqlSecureQueryParameter> parameters, SqlModificationBuilder exists, SqlModificationBuilder notExists = null) : base(plan, configurationOptions, parameters)
            {
                _initialize(plan, exists, notExists);
            }

            private void _addWhere(IModificationItem item, string parameterKey)
            {
                var wherePrefix = string.IsNullOrEmpty(_where) ? string.Empty : " AND ";
                _where = string.Concat(_where, string.Format("{0}[{1}] = {2}", wherePrefix, item.DatabaseColumnName, parameterKey));
            }

            private void _initialize(ISqlExecutionPlan plan, SqlModificationBuilder exists, SqlModificationBuilder notExists)
            {
                _exists = exists;
                _notExists = notExists;
                _where = string.Empty;

                // combine parameters

                // keys are not part of changes so we need to grab them
                var primaryKeys = Entity.Keys();

                // add where statement
                for (var i = 0; i < primaryKeys.Count; i++)
                {
                    var key = primaryKeys[i];
                    var value = Entity.GetPropertyValue(key.PropertyName);
                    var parameter = AddParameter(key, value);

                    _addWhere(key, parameter);
                }

                _existsStatement = string.Format(@"IF (NOT(EXISTS(SELECT TOP 1 1 FROM [{0}] WHERE {1}))) 
    BEGIN
        {{0}}
    END {2}", plan.Entity.ToString(TableNameFormat.SqlWithSchemaTrimStartAndEnd), _where, notExists == null ? string.Empty : @"
ELSE
    BEGIN 
        {1} 
    END");
            }

            // not used
            protected override ISqlContainer NewContainer()
            {
                return _notExists != null ? new CustomContainer(Entity, string.Format(_existsStatement, _exists.GetSql(), _notExists.GetSql())) : new CustomContainer(Entity, string.Format(_existsStatement, _exists.GetSql()));
            }

            public override ISqlContainer BuildContainer()
            {
                return NewContainer();
            }
        }

        private class SqlInsertBuilder : SqlModificationBuilder
        {
            #region Constructor

            public SqlInsertBuilder(ISqlExecutionPlan builder, IConfigurationOptions configurationOptions, List<SqlSecureQueryParameter> parameters) : base(builder, configurationOptions, parameters)
            {
            }

            #endregion

            #region Methods

            protected override ISqlContainer NewContainer()
            {
                return new InsertContainer(Entity);
            }

            protected virtual void AddDbGenerationOptionNone(ISqlContainer container, IModificationItem item)
            {
                // timestamps are never inserted, they are generated by the database 
                // then read back into the instance being saved
                if (item.DbTranslationType == SqlDbType.Timestamp) return;

                // parameter key
                var value = Entity.GetPropertyValue(item.PropertyName);
                var data = AddParameter(item, value);

                ((InsertContainer) container).AddField(item);
                ((InsertContainer) container).AddValue(data);
            }

            protected virtual void AddDbGenerationOptionGenerate(ISqlContainer container, IModificationItem item)
            {
                // key from the set method
                string key;

                ((InsertContainer) container).AddSetGenerationItem(item, out key);
                ((InsertContainer) container).AddField(item);
                ((InsertContainer) container).AddValue(key);
                ((InsertContainer) container).AddDeclare(key, item.SqlDataTypeString);
            }

            protected virtual void AddOutput(ISqlContainer container, IModificationItem item)
            {
                ((InsertContainer) container).AddOutput(item);
            }

            public override ISqlContainer BuildContainer()
            {
                var items = Entity.All();
                var container = NewContainer();

                if (items.Count == 0) throw new QueryNotValidException("INSERT statement needs VALUES");

                for (var i = 0; i < items.Count; i++)
                {
                    var item = items[i];

                    // we want to select back the whole inserted object
                    AddOutput(container, item);

                    //  NOTE:  Alias any Identity specification and generate columns with their property
                    // name not db column name so we can set the property when we return the values back.
                    switch (item.Generation)
                    {
                        case DbGenerationOption.None:
                            AddDbGenerationOptionNone(container, item);
                            break;
                        case DbGenerationOption.Generate:
                            AddDbGenerationOptionGenerate(container, item);
                            break;
                        // These will get added by default, because
                        // we select back the whole object
                        //case DbGenerationOption.DbDefault:
                        //case DbGenerationOption.IdentitySpecification:
                        default:
                            continue;
                    }
                }

                return container;
            }

            #endregion
        }

        private class SqlUpdateBuilder : SqlModificationBuilder
        {
            #region Constructor

            public SqlUpdateBuilder(ISqlExecutionPlan builder, IConfigurationOptions configurationOptions, List<SqlSecureQueryParameter> parameters) : base(builder, configurationOptions, parameters)
            {
            }

            #endregion

            #region Methods

            protected override ISqlContainer NewContainer()
            {
                return new UpdateContainer(Entity);
            }

            protected virtual void AddUpdate(ISqlContainer container, IModificationItem item)
            {
                // These instances cannot be updated, we should read them back and insert the original value into the entity
                if (item.Generation == DbGenerationOption.IdentitySpecification || item.DbTranslationType == SqlDbType.Timestamp) return;

                var value = Entity.GetPropertyValue(item.PropertyName);
                var parameter = AddParameter(item, value);

                ((UpdateContainer) container).AddUpdate(item, parameter);
            }

            protected virtual void AddWhere(ISqlContainer container, IModificationItem item)
            {
                var value = Entity.GetPropertyValue(item.PropertyName);
                var parameter = AddParameter(item, value);

                // PK cannot be null here
                ((UpdateContainer) container).AddWhere(item, parameter);
            }

            protected virtual void AddOutput(ISqlContainer container, IModificationItem item)
            {
                ((UpdateContainer) container).AddOutput(item);
            }

            public override ISqlContainer BuildContainer()
            {
                var items = Entity.All();
                var container = (UpdateContainer) NewContainer();

                // if we got here there are columns to update, the entity is analyzed before this method.  Check again anyways
                if (items.Count == 0) throw new SqlSaveException("No items to update, query analyzer failed");

                for (var i = 0; i < items.Count; i++)
                {
                    var item = items[i];

                    if (item.IsPrimaryKey)
                    {
                        AddWhere(container, item);
                        continue;
                    }

                    // select back the whole object, someone else might
                    // have updated a different field than what the user 
                    // is updating
                    AddOutput(container, item);

                    if (!item.IsModified) continue;

                    // set the item to be updated
                    AddUpdate(container, item);


                    // check to see if concurrency checking is on
                    if (!Configuration.ConcurrencyChecking.IsOn) continue;

                    // cannot concurrency check if entity state tracking is not on 
                    // because we do not know what the user has and has not changed. If entity state tracking is on and the 
                    // pristine entity is null then we have a try insert update, make sure to skip the concurrency check
                    if (!Entity.IsEntityStateTrackingOn || item.Generation == DbGenerationOption.IdentitySpecification || (Entity.IsEntityStateTrackingOn && Entity.IsPristineEntityNull()))
                    {
                        continue;
                    }

                    // concurrency check
                    var concurrencyValue = Entity.GetPristineEntityPropertyValue(item.PropertyName);
                    var isConcurrencyValueNull = concurrencyValue == null;

                    if (!isConcurrencyValueNull)
                    {
                        // concurrency is only checked in an update so we do not need to use TryAddParameter
                        var concurrencyParameter = AddPristineParameter(item, concurrencyValue);
                        container.AddWhere(item, concurrencyParameter);
                        continue;
                    }

                    // property is null, make a different check
                    container.AddNullWhere(item);
                }

                return container;
            }

            #endregion
        }

        private class SqlDeleteBuilder : SqlModificationBuilder
        {
            #region Constructor

            public SqlDeleteBuilder(ISqlExecutionPlan builder, IConfigurationOptions configurationOptions, List<SqlSecureQueryParameter> parameters) : base(builder, configurationOptions, parameters)
            {
            }

            #endregion

            #region Methods

            protected override ISqlContainer NewContainer()
            {
                return new DeleteContainer(Entity);
            }

            protected virtual void AddWhere(ISqlContainer container, IModificationItem item)
            {
                var value = Entity.GetPropertyValue(item.PropertyName);
                var parameter = AddParameter(item, value);

                // PK can be null here
                if (value == null)
                {
                    ((DeleteContainer) container).AddNullWhere(item);
                    return;
                }

                ((DeleteContainer) container).AddWhere(item, parameter);
            }

            public override ISqlContainer BuildContainer()
            {
                var container = (DeleteContainer) NewContainer();

                // keys are not part of changes so we need to grab them
                var primaryKeys = Entity.Keys();

                // add where statement
                for (var i = 0; i < primaryKeys.Count; i++)
                {
                    var key = primaryKeys[i];

                    AddWhere(container, key);
                }

                return container;
            }

            #endregion
        }

        #endregion

        #region Methods

        /// <summary>
        /// returns true if anything was modified and false if no changes were made
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="entity"></param>
        /// <returns></returns>
        private IPersistResult _saveChanges<T>(T entity) where T : class
        {
            try
            {
                var parent = new ModificationEntity(entity, string.Empty, Configuration, DbTableFactory);
                var changeManager = new ChangeManager();

                // get all items to save and get them in order
                var referenceMap = EntityMapper.GetReferenceMap(parent, Configuration, DbTableFactory, false);

                for (var i = 0; i < referenceMap.Count; i++)
                {
                    var reference = referenceMap[i];
                    ISqlExecutionPlan plan;

                    if (OnBeforeSave != null) OnBeforeSave(reference.Entity.Value, reference.Entity.UpdateType);

                    // add table and update type
                    changeManager.AddTable(reference.Entity.PlainTableName, reference.Entity.UpdateType);

                    // Get the correct execution plan
                    switch (reference.Entity.UpdateType)
                    {
                        case UpdateType.Insert:
                            plan = new SqlInsertPlan(reference.Entity, Configuration);
                            break;
                        case UpdateType.TryInsert:
                            plan = new SqlTryInsertPlan(reference.Entity, Configuration);
                            break;
                        case UpdateType.TryInsertUpdate:
                            plan = new SqlTryInsertUpdatePlan(reference.Entity, Configuration);
                            break;
                        case UpdateType.Update:
                            plan = new SqlUpdatePlan(reference.Entity, Configuration);
                            break;
                        case UpdateType.Skip:
                            continue;
                        default:
                            throw new ArgumentOutOfRangeException();
                    }

                    // If relationship is one-many.  Need to set the foreign key before saving
                    if (reference.Parent != null && reference.Property.IsList())
                    {
                        Entity.SetPropertyValue(reference.Parent, reference.Entity.Value, reference.Property.Name);
                    }

                    if (OnSaving != null) OnSaving(reference.Entity.Value);

                    // execute the sql
                    ExecuteReader(plan);

                    // get output and clean up connections
                    var keyContainer = GetOutput(reference.Entity, changeManager);

                    // make sure data was inserted, if not set the object back on the update manager.
                    // might be a sql server error
                    if (keyContainer.Count == 0) changeManager.ChangeUpdateType(reference.Entity.PlainTableName, UpdateType.Skip);

                    // check for concurrency violation
                    var hasConcurrencyViolation = false;
                    var isConcurrencyViolationCandidate = reference.Entity.UpdateType == UpdateType.Update && keyContainer.Count == 0 && Configuration.ConcurrencyChecking.IsOn;

                    // if we are a candidate, we need to make sure the record exists.
                    // if no record exists then there is no violation.
                    // processing for concurrency violation
                    if (isConcurrencyViolationCandidate && Exists(entity))
                    {
                        // mark the violation
                        hasConcurrencyViolation = true;

                        // violation occurred, choose what to do
                        switch (Configuration.ConcurrencyChecking.ViolationRule)
                        {
                            case ConcurrencyViolationRule.ThrowException:
                                throw new DBConcurrencyException(string.Format("Concurrency Violation.  {0} was changed prior to this update", reference.Entity.ToString(TableNameFormat.Plain)));
                            case ConcurrencyViolationRule.OverwriteAndContinue:
                                break;
                            case ConcurrencyViolationRule.UseHandler:
                                if (OnConcurrencyViolation != null) OnConcurrencyViolation(reference.Entity.Value);
                                break;
                        }
                    }

                    // put updated values into entity
                    foreach (var item in keyContainer)
                    {
                        // find the property first in case the column name change attribute is used
                        // Key is property name, value is the db value
                        reference.Entity.SetPropertyValue(item.Key, item.Value);
                    }

                    // If relationship is one-one.  Need to set the foreign key after saving
                    if (reference.Parent != null && !reference.Property.IsList())
                    {
                        Entity.SetPropertyValue(reference.Parent, reference.Entity.Value, reference.Property.Name);
                    }

                    // set the pristine state only if entity tracking is on and there is no concurrency violation
                    if (reference.Entity.IsEntityStateTrackingOn && !hasConcurrencyViolation) ModificationEntity.TrySetPristineEntity(reference.Entity.Value);

                    if (OnAfterSave != null) OnAfterSave(reference.Entity.Value, reference.Entity.UpdateType);
                }

                return changeManager.Compile();
            }
            catch (MaxLengthException ex)
            {
                // only catch the max length exception so we can tell the user that the save was cancelled
                throw new SqlSaveException("Max length violated, see inner exception", ex);
            }
        }

        private IPersistResult _delete<T>(T entity) where T : class
        {
            var parent = new ModificationEntity(entity, string.Empty, Configuration, DbTableFactory);
            var changeManager = new ChangeManager();

            // get all items to save and get them in order
            var referenceMap = EntityMapper.GetReferenceMap(parent, Configuration, DbTableFactory, true);

            // reverse the order to back them out of the database
            referenceMap.Reverse();

            for (var i = 0; i < referenceMap.Count; i++)
            {
                var reference = referenceMap[i];

                if (OnBeforeSave != null) OnBeforeSave(reference.Entity.Value, UpdateType.Delete);

                var builder = new SqlDeletePlan(reference.Entity, Configuration);

                if (OnSaving != null) OnSaving(reference.Entity.Value);

                // execute the sql
                ExecuteReader(builder);

                // we return the deleted id's to check and see if anything was deleted
                var actionTaken = Reader.HasRows ? UpdateType.Delete : UpdateType.RowNotFound;

                // clean up connection
                Disconnect();

                // add result to change manager
                changeManager.AddTable(reference.Entity.PlainTableName, actionTaken);

                // set the pristine state only if entity tracking is on
                if (reference.Entity.IsEntityStateTrackingOn) ModificationEntity.TrySetPristineEntity(reference.Entity.Value);

                if (OnAfterSave != null) OnAfterSave(reference.Entity.Value, actionTaken);
            }

            return changeManager.Compile();
        }

        #endregion
    }
}
