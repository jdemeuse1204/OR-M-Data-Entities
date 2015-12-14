using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Linq;
using OR_M_Data_Entities.Data.Definition;
using OR_M_Data_Entities.Data.Modification;
using OR_M_Data_Entities.Data.Query;
using OR_M_Data_Entities.Data.Secure;
using OR_M_Data_Entities.Enumeration;
using OR_M_Data_Entities.Exceptions;

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
        #endregion

        #region Containers
        private class TransactionInsertContainer : InsertContainer
        {
            private string _tableVariable { get; set; }

            private readonly string _tableAlias;

            public TransactionInsertContainer(ModificationEntity entity, string tableAlias)
                : base(entity)
            {
                _tableAlias = tableAlias;
            }

            public void AddTableVariable(ModificationItem item)
            {
                _tableVariable = string.Concat(_tableVariable, string.Format("{0} {1},", item.PropertyName, item.SqlDataTypeString));
            }

            public override SqlPartStatement Split()
            {
                var sql = string.Format("INSERT INTO [{0}] ({1}) OUTPUT {2} INTO @{3} VALUES ({4});\rSELECT {5} FROM @{3}",
                    SqlFormattedTableName,
                    _fields.TrimEnd(','),
                    _output.TrimEnd(','),
                    _tableAlias,
                    _values.TrimEnd(','),
                    _outputColumnsOnly.TrimEnd(',')

                    // we want to select everything back from the database in case the model relies on db generation for some fields.
                    // this way we can load the data back into the model.  Works perfect for things like time stamps and auto generation
                    // where the column is not the PK
                    );

                _declare = string.Concat(_declare, string.Format("DECLARE @{0} TABLE({1});\r", _tableAlias, _tableVariable.TrimEnd(',')));

                return new SqlPartStatement(sql, _declare, _set);
            }
        }
        #endregion

        #region Builder
        private class SqlTransactionBuilder : ISqlBuilder
        {
            private readonly ReferenceMap _referenceMap;

            private readonly List<ISqlBuilder> _builders;

            private readonly List<SqlSecureQueryParameter> _parameters;

            public SqlTransactionBuilder(ReferenceMap map, List<SqlSecureQueryParameter> parameters)
            {
                _referenceMap = map;
                _builders = new List<ISqlBuilder>();
                _parameters = parameters;
            }

            public void Add<T>(T builder)
                where T : ISqlBuilder, ISqlTransactionBuilder
            {
                _builders.Add(builder);
            }

            public ISqlPackage Build()
            {
                if (_builders == null || _builders.Count == 0)
                {
                    throw new SqlSaveException("No items to save");
                }

                return new SqlTransactionPackage(_referenceMap, _builders, _parameters);
            }

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

            public ModificationEntity Entity { get; private set; }
        }

        private class SqlTransactionInsertBuilder : SqlInsertBuilder, ISqlTransactionBuilder
        {
            private readonly Reference _reference;

            public SqlTransactionInsertBuilder(ModificationEntity entity, List<SqlSecureQueryParameter> sharedParameters, Reference reference)
                : base(entity, sharedParameters)
            {
                _reference = reference;
            }

            public override ISqlPackage Build()
            {
                return new SqlTransactionInsertPackage(this, Parameters, _reference);
            }
        }

        private class SqlTransactionTryInsertBuilder : SqlTryInsertBuilder, ISqlTransactionBuilder
        {
            private readonly Reference _reference;

            public SqlTransactionTryInsertBuilder(ModificationEntity entity, List<SqlSecureQueryParameter> sharedParameters, Reference reference)
                : base(entity, sharedParameters)
            {
                _reference = reference;
            }
        }

        private class SqlTransactionTryInsertUpdateBuilder : SqlTryInsertUpdateBuilder, ISqlTransactionBuilder
        {
            private readonly Reference _reference;

            public SqlTransactionTryInsertUpdateBuilder(ModificationEntity entity, List<SqlSecureQueryParameter> parameters, Reference reference)
                : base(entity, parameters)
            {
                _reference = reference;
            }
        }

        private class SqlTransactionUpdateBuilder : SqlUpdateBuilder, ISqlTransactionBuilder
        {
            private readonly Reference _reference;

            public SqlTransactionUpdateBuilder(ModificationEntity entity, List<SqlSecureQueryParameter> parameters, Reference reference)
                : base(entity, parameters)
            {
                _reference = reference;
            }
        }

        private class SqlTransactionDeleteBuilder : SqlDeleteBuilder, ISqlTransactionBuilder
        {
            private readonly Reference _reference;

            public SqlTransactionDeleteBuilder(ModificationEntity entity, List<SqlSecureQueryParameter> parameters, Reference reference)
                : base(entity, parameters)
            {
                _reference = reference;
            }
        }

        // for constraint only
        private interface ISqlTransactionBuilder
        {

        }
        #endregion

        #region Package

        private class SqlTransactionPackage : ISqlPackage
        {
            private readonly List<ISqlBuilder> _builders;

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

            public SqlTransactionPackage(ReferenceMap referenceMap, List<ISqlBuilder> builders, List<SqlSecureQueryParameter> parameters)
            {
                _builders = builders;
                _referenceMap = referenceMap;
                _parameters = parameters;
            }

            public string GetSql()
            {
                var container = CreatePackage();

                return container.Resolve();
            }

            public ISqlContainer CreatePackage()
            {
                var sql = string.Empty;
                var declare = string.Empty;
                var set = string.Empty;

                // create our transaction package here
                for (var i = 0; i < _builders.Count; i++)
                {
                    var builder = _builders[i];

                    // build the package
                    var package = builder.Build();

                    // create the container
                    var container = package.CreatePackage();

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

        private class SqlTransactionInsertPackage : SqlInsertPackage
        {
            private readonly Reference _reference;

            public SqlTransactionInsertPackage(ISqlBuilder builder, List<SqlSecureQueryParameter> parameters, Reference reference)
                : base(builder, parameters)
            {
                _reference = reference;
            }

            private ReferenceNode _getReferenceNode(ModificationItem item)
            {
                return _reference.References.FirstOrDefault(w => w.Link.ParentColumnName == item.DatabaseColumnName);
            }

            protected override ISqlContainer NewContainer()
            {
                return new TransactionInsertContainer(Entity, _reference.Alias);
            }

            protected override void AddDbGenerationOptionNone(ISqlContainer container, ModificationItem item)
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
                var data = TryAddParameter(item, value);

                ((TransactionInsertContainer)container).AddValue(data);
            }

            protected override void AddDbGenerationOptionGenerate(ISqlContainer container, ModificationItem item)
            {
                ((TransactionInsertContainer)container).AddTableVariable(item);

                base.AddDbGenerationOptionGenerate(container, item);
            }

            protected override void AddDbGenerationOptionIdentityAndDefault(ISqlContainer container, ModificationItem item)
            {
                ((TransactionInsertContainer)container).AddTableVariable(item);

                base.AddDbGenerationOptionIdentityAndDefault(container, item);
            }
        }
        #endregion

        #region Methods
        private void _loadObjectFromMultipleActiveResults(ReferenceMap referenceMap)
        {
            if (!Reader.HasRows)
            {
                // close reader and connection
                Connection.Close();
                Reader.Close();
                Reader.Dispose();
                return;
            }

            var i = 0;

            do
            {
                // reference map and result sets are in the same order
                var reference = referenceMap[i];

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

                i++;

            } while (Reader.NextResult());

            Connection.Close();
            Reader.Close();
            Reader.Dispose();
        }

        public virtual bool _saveChangesUsingTransactions<T>(T entity) where T : class
        {
            var saves = new List<UpdateType>();
            var parent = new ModificationEntity(entity);

            // get all items to save and get them in order
            var referenceMap = EntityMapper.GetReferenceMap(parent, Configuration);
            var parameters = new List<SqlSecureQueryParameter>();
            var builder = new SqlTransactionBuilder(referenceMap, parameters);

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
                        builder.Add(new SqlTransactionInsertBuilder(reference.Entity, parameters, reference));
                        break;
                    case UpdateType.TryInsert:
                        builder.Add(new SqlTransactionTryInsertBuilder(reference.Entity, parameters, reference));
                        break;
                    case UpdateType.TryInsertUpdate:
                        builder.Add(new SqlTransactionTryInsertUpdateBuilder(reference.Entity, parameters, reference));
                        break;
                    case UpdateType.Update:
                        builder.Add(new SqlTransactionUpdateBuilder(reference.Entity, parameters, reference));
                        break;
                    case UpdateType.Skip:
                        continue;
                    default:
                        throw new ArgumentOutOfRangeException();
                }
            }

            if (OnSaving != null) OnSaving(entity);

            // execute the sql
            ExecuteReader(builder);

            _loadObjectFromMultipleActiveResults(referenceMap);

            // set the pristine state only if entity tracking is on
            if (parent.IsEntityStateTrackingOn) ModificationEntity.TrySetPristineEntity(entity);

            if (OnAfterSave != null) OnAfterSave(entity, UpdateType.TransactionalSave);

            return saves.Any(w => w != UpdateType.Skip);
        }
        #endregion
    }
}
