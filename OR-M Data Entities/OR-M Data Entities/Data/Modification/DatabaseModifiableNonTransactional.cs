using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Linq;
using OR_M_Data_Entities.Data.Definition;
using OR_M_Data_Entities.Data.Modification;
using OR_M_Data_Entities.Data.Query;
using OR_M_Data_Entities.Data.Secure;
using OR_M_Data_Entities.Exceptions;
using OR_M_Data_Entities.Extensions;
using OR_M_Data_Entities.Mapping;

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

            public CustomContainer(ModificationEntity entity, string sql)
                : base(entity)
            {
                _sql = sql;
            }

            public string Resolve()
            {
                return Split().ToString();
            }

            public SqlPartStatement Split()
            {
                return new SqlPartStatement(_sql);
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
            public InsertContainer(ModificationEntity entity)
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
                _fields = string.Concat(_fields, item.AsField(","));
            }

            public void AddValue(string parameterKey)
            {
                _values = string.Concat(_values, string.Format("{0},", parameterKey));
            }

            public void AddDeclare(string parameterKey, string sqlDataType)
            {
                _declare = string.Concat(_declare, string.Format("{0} as {1},", parameterKey, sqlDataType));
            }

            public void AddOutput(ModificationItem item)
            {
                _output = string.Concat(_output, item.AsOutput(","));
            }

            public void AddSet(ModificationItem item, out string key)
            {
                key = string.Format("@{0}", item.PropertyName);

                // make our set statement
                if (item.SqlDataTypeString.ToUpper() == "UNIQUEIDENTIFIER")
                {
                    // GUID
                    _set = string.Concat(_set, string.Format("SET {0} = NEWID();", key));
                }
                else
                {
                    // NUMERIC
                    _set = string.Concat(_set,
                        string.Format("SET {0} = (Select ISNULL(MAX([{1}]),0) + 1 From [{2}]);", key,
                            item.DatabaseColumnName, SqlFormattedTableName));
                }
            }

            public string Resolve()
            {
                return Split().ToString();
            }

            public SqlPartStatement Split()
            {
                var sql = string.Format("INSERT INTO [{0}] ({1}) OUTPUT {2} VALUES ({3})",
                    SqlFormattedTableName,
                    _fields.TrimEnd(','),
                    _output.TrimEnd(','),
                    _values.TrimEnd(',')

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
                _setItems = string.Concat(_setItems, string.Format("[{0}] = {1},", item.DatabaseColumnName, parameterKey));
            }

            public void AddWhere(ModificationItem item, string parameterKey)
            {
                _where = string.Concat(_where, string.Format("{0}[{1}] = {2}", _getWherePrefix(), item.DatabaseColumnName, parameterKey));
            }

            public void AddNullWhere(ModificationItem item)
            {
                _where = string.Concat(_where, string.Format("{0}[{1}] IS NULL", _getWherePrefix(), item.DatabaseColumnName));
            }

            private string _getWherePrefix()
            {
                return string.IsNullOrEmpty(_where) ? string.Empty : " AND ";
            }

            public string Resolve()
            {
                return Split().ToString();
            }

            public SqlPartStatement Split()
            {
                var sql = string.Format("{0} SET {1} WHERE {2}", _statement, _setItems.TrimEnd(','), _where.TrimEnd(','));
                
                return new SqlPartStatement(sql);
            }
        }

        private class DeleteContainer : SqlModificationContainer, ISqlContainer
        {
            private string _where { get; set; }

            private readonly string _output;

            private readonly string _statement;

            public DeleteContainer(ModificationEntity entity)
                : base(entity)
            {
                _output = entity.GetPrimaryKeys()
                    .Select(w => w.GetColumnName())
                    .Aggregate("OUTPUT ", (s, s1) => string.Concat(s, string.Format("[DELETED].[{0}],", s1)))
                    .TrimEnd(',');
                
                _where = string.Empty;
                _statement = string.Format("DELETE FROM [{0}]", SqlFormattedTableName);
            }

            public void AddWhere(ModificationItem item, string parameterKey)
            {
                _where = string.Concat(_where, string.Format("{0}[{1}] = {2}", _getWherePrefix(), item.DatabaseColumnName, parameterKey));
            }

            public void AddNullWhere(ModificationItem item)
            {
                _where = string.Concat(_where, string.Format("{0}[{1}] IS NULL", _getWherePrefix(), item.DatabaseColumnName));
            }

            private string _getWherePrefix()
            {
                return string.IsNullOrEmpty(_where) ? string.Empty : " AND ";
            }

            public string Resolve()
            {
                return Split().ToString();
            }

            public SqlPartStatement Split()
            {
                var sql = string.Format("{0} {1} WHERE {2}", _statement, _output, _where.TrimEnd(','));

                return new SqlPartStatement(sql);
            }
        }

        protected abstract class SqlModificationContainer
        {
            protected SqlModificationContainer(ModificationEntity entity)
            {
                SqlFormattedTableName = entity.SqlFormattedTableName();
            }

            protected readonly string SqlFormattedTableName;
        }
        #endregion

        #region Base
        /// <summary>
        /// Provides us a way to get the execution plan for an entity
        /// </summary>
        private abstract class SqlExecutionPlan : ISqlBuilder
        {
            #region Constructor
            protected SqlExecutionPlan(ModificationEntity entity)
            {
                Entity = entity;
            }
            #endregion

            #region Properties and Fields
            public ModificationEntity Entity { get; private set; }
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
            protected SqlModificationPackage(ISqlBuilder plan)
            {
                Entity = plan.Entity;
            }

            protected SqlModificationPackage(ISqlBuilder plan, List<SqlSecureQueryParameter> parameters)
                : base(parameters)
            {
                Entity = plan.Entity;
            }

            protected string TryAddParameter(ModificationItem item, object value)
            {
                var foundKey = FindParameterKey(item.DatabaseColumnName);

                return !string.IsNullOrWhiteSpace(foundKey)
                    ? foundKey
                    : AddParameter(item, value);
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

        #region Builders
        private class SqlInsertBuilder : SqlExecutionPlan
        {
            public SqlInsertBuilder(ModificationEntity entity)
                : base(entity)
            {
            }

            public override ISqlPackage Build()
            {
                return new SqlInsertPackage(this);
            }
        }

        private class SqlTryInsertBuilder : SqlExecutionPlan
        {
            public SqlTryInsertBuilder(ModificationEntity entity)
                : base(entity)
            {
            }

            public override ISqlPackage Build()
            {
                var parameters = new List<SqlSecureQueryParameter>();
                var insert = new SqlInsertPackage(this, parameters);

                return new SqlExistsPackage(this, parameters, insert);
            }
        }

        private class SqlTryInsertUpdateBuilder : SqlExecutionPlan
        {
            public SqlTryInsertUpdateBuilder(ModificationEntity entity)
                : base(entity)
            {
            }

            public override ISqlPackage Build()
            {
                // insert and update need to share (by reference) their parameters list so they are in sync and do not overlap keys
                var parameters = new List<SqlSecureQueryParameter>();
                var insert = new SqlInsertPackage(this, parameters);
                var update = new SqlUpdatePackage(this, parameters);

                return new SqlExistsPackage(this, parameters, insert, update);
            }
        }

        private class SqlUpdateBuilder : SqlExecutionPlan
        {
            public SqlUpdateBuilder(ModificationEntity entity)
                : base(entity)
            {
            }

            public override ISqlPackage Build()
            {
                return new SqlUpdatePackage(this);
            }
        }

        private class SqlDeleteBuilder : SqlExecutionPlan
        {
            public SqlDeleteBuilder(ModificationEntity entity)
                : base(entity)
            {
            }

            public override ISqlPackage Build()
            {
                return new SqlDeletePackage(this);
            }
        }
        #endregion

        #region Packages
        private class SqlExistsPackage : SqlModificationPackage
        {
            private SqlModificationPackage _exists { get; set; }

            private SqlModificationPackage _notExists { get; set; }

            private string _existsStatement { get; set; }

            private string _where { get; set; }

            public SqlExistsPackage(ISqlBuilder plan, List<SqlSecureQueryParameter> parameters, SqlModificationPackage exists, SqlModificationPackage notExists = null)
                : base(plan, parameters)
            {
                _initialize(plan, exists, notExists);
            }

            private void _addWhere(ModificationItem item, string parameterKey)
            {
                var wherePrefix = string.IsNullOrEmpty(_where) ? string.Empty : " AND ";
                _where = string.Concat(_where, string.Format("{0}[{1}] = {2}", wherePrefix, item.DatabaseColumnName, parameterKey));
            }

            private void _initialize(ISqlBuilder plan, SqlModificationPackage exists, SqlModificationPackage notExists)
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
                    var parameter = TryAddParameter(key, value);

                    _addWhere(key, parameter);
                }

                _existsStatement = string.Format(@"IF (NOT(EXISTS(SELECT TOP 1 1 FROM [{0}] WHERE {1}))) 
    BEGIN
        {{0}}
    END {2}",
plan.Entity.SqlFormattedTableName(),

_where,

notExists == null ?
string.Empty :
@"
ELSE
    BEGIN 
        {1} 
    END");
            }

            public override ISqlContainer CreatePackage()
            {
                return _notExists != null
                    ? new CustomContainer(Entity, string.Format(_existsStatement, _exists.GetSql(), _notExists.GetSql()))
                    : new CustomContainer(Entity, string.Format(_existsStatement, _exists.GetSql()));
            }
        }

        private class SqlInsertPackage : SqlModificationPackage
        {
            #region Constructor
            public SqlInsertPackage(ISqlBuilder builder)
                : base(builder)
            {

            }

            public SqlInsertPackage(ISqlBuilder builder, List<SqlSecureQueryParameter> parameters)
                : base(builder, parameters)
            {

            }
            #endregion

            #region Methods
            private void _addDbGenerationOptionNone(InsertContainer container, ModificationItem item)
            {
                if (item.DbTranslationType == SqlDbType.Timestamp)
                {
                    container.AddOutput(item);
                    return;
                }

                // parameter key
                var value = Entity.GetPropertyValue(item.PropertyName);
                var data = TryAddParameter(item, value);

                container.AddField(item);
                container.AddValue(data);
                container.AddOutput(item);
            }

            private void _addDbGenerationOptionGenerate(InsertContainer container, ModificationItem item)
            {
                // key from the set method
                string key;

                container.AddSet(item, out key);
                container.AddField(item);
                container.AddValue(key);
                container.AddDeclare(key, item.SqlDataTypeString);
                container.AddOutput(item);
            }

            private void _addDbGenerationOptionIdentityAndDefault(InsertContainer container, ModificationItem item)
            {
                container.AddOutput(item);
            }

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
                            _addDbGenerationOptionNone(container, item);
                            break;
                        case DbGenerationOption.Generate:
                            _addDbGenerationOptionGenerate(container, item);
                            break;
                        case DbGenerationOption.DbDefault:
                        case DbGenerationOption.IdentitySpecification:
                            _addDbGenerationOptionIdentityAndDefault(container, item);
                            break;
                    }
                }

                return container;
            }
            #endregion
        }

        private class SqlUpdatePackage : SqlModificationPackage
        {
            #region Constructor

            public SqlUpdatePackage(ISqlBuilder builder, List<SqlSecureQueryParameter> parameters)
                : base(builder, parameters)
            {
            }

            public SqlUpdatePackage(ISqlBuilder builder)
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

                    var parameter = TryAddParameter(item, value);

                    container.AddUpdate(item, parameter);

                    // cannot concurrency check if entity state tracking is not on 
                    // because we do not know what the user has and has not changed
                    if (!Entity.IsEntityStateTrackingOn) continue;

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

                // keys are not part of changes so we need to grab them
                var primaryKeys = Entity.Keys();

                // add where statement
                for (var i = 0; i < primaryKeys.Count; i++)
                {
                    var key = primaryKeys[i];
                    var value = Entity.GetPropertyValue(key.PropertyName);
                    var parameter = TryAddParameter(key, value);

                    // PK cannot be null here
                    container.AddWhere(key, parameter);
                }

                return container;
            }
            #endregion
        }

        private class SqlDeletePackage : SqlModificationPackage
        {
            #region Constructor

            public SqlDeletePackage(ISqlBuilder builder, List<SqlSecureQueryParameter> parameters)
                : base(builder, parameters)
            {
            }

            public SqlDeletePackage(ISqlBuilder builder)
                : base(builder)
            {
            }
            #endregion

            #region Methods
            public override ISqlContainer CreatePackage()
            {
                var container = new DeleteContainer(Entity);

                // keys are not part of changes so we need to grab them
                var primaryKeys = Entity.Keys();

                // add where statement
                for (var i = 0; i < primaryKeys.Count; i++)
                {
                    var key = primaryKeys[i];
                    var value = Entity.GetPropertyValue(key.PropertyName);
                    var parameter = TryAddParameter(key, value);

                    // PK can be null here
                    if (value == null)
                    {
                        container.AddNullWhere(key);
                        continue;
                    }

                    container.AddWhere(key, parameter);
                }

                return container;
            }
            #endregion
        }
        #endregion
    }
}
