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
            public SqlTransactionContainer() 
            {
            }

            public string Resolve()
            {
                throw new NotImplementedException();
            }

            public SqlPartStatement Split()
            {
                throw new NotImplementedException();
            }
        }
        #endregion

        #region Containers
        private class TransactionInsertContainer : InsertContainer
        {
            private string _tableVariable { get; set; }

            public TransactionInsertContainer(ModificationEntity entity)
                : base(entity)
            {
            }

            public void AddTableVariable(ModificationItem item)
            {
                _tableVariable = string.Concat(_tableVariable, string.Format("{0} {1},", item.DatabaseColumnName, item.SqlDataTypeString));
            }

            public override SqlPartStatement Split()
            {
                var sql = string.Format("INSERT INTO [{0}] ({1}) OUTPUT {2} INTO TABLE({3}) VALUES ({4})",
                    SqlFormattedTableName,
                    _fields.TrimEnd(','),
                    _output.TrimEnd(','),
                    _tableVariable.TrimEnd(','),
                    _values.TrimEnd(',')

                    // we want to select everything back from the database in case the model relies on db generation for some fields.
                    // this way we can load the data back into the model.  Works perfect for things like time stamps and auto generation
                    // where the column is not the PK
                    );

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
                // create our transaction package here
                for (var i = 0; i < _builders.Count; i++)
                {
                    var builder = _builders[i];
                    var package = builder.Build();

                    package.CreatePackage();
                }

                return new SqlTransactionContainer();
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

            private bool _hasReference(ModificationItem item)
            {



                return false;
            }

            protected override ISqlContainer NewContainer()
            {
                return new TransactionInsertContainer(Entity);
            }

            protected override void AddDbGenerationOptionNone(ISqlContainer container, ModificationItem item)
            {
                if (item.DbTranslationType == SqlDbType.Timestamp)
                {
                    ((TransactionInsertContainer)container).AddOutput(item);
                    return;
                }

                ((TransactionInsertContainer)container).AddField(item);
                ((TransactionInsertContainer)container).AddOutput(item);

                if (_hasReference(item))
                {
                    // grab the reference from the other table
                    return;
                }

                // parameter key
                var value = Entity.GetPropertyValue(item.PropertyName);
                var data = TryAddParameter(item, value);

                ((TransactionInsertContainer)container).AddValue(data);
            }
        }
        #endregion

        #region Methods

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

                // If relationship is one-many.  Need to set the foreign key before saving
                //if (reference.Parent != null && reference.Property.IsList())
                //{
                //    Entity.SetPropertyValue(reference.Parent, reference.Entity.Value, reference.Property.Name);
                //}

                if (OnSaving != null) OnSaving(reference.Entity.Value);

                // execute the sql
                ExecuteReader(builder);

                var keyContainer = GetOutput();

                // check if a concurrency violation has occurred
                if (reference.Entity.UpdateType == UpdateType.Update && keyContainer.Count == 0 &&
                    Configuration.ConcurrencyViolationRule == ConcurrencyViolationRule.ThrowException)
                {
                    throw new DBConcurrencyException("Concurrency Violation.  {0} was changed prior to this update");
                }

                // put updated values into entity
                foreach (var item in keyContainer)
                {
                    // find the property first in case the column name change attribute is used
                    // Key is property name, value is the db value
                    reference.Entity.SetPropertyValue(
                        item.Key,
                        item.Value);
                }

                // If relationship is one-one.  Need to set the foreign key after saving
                if (reference.Parent != null && !reference.Property.IsList())
                {
                    Entity.SetPropertyValue(reference.Parent, reference.Entity.Value, reference.Property.Name);
                }

                // set the pristine state only if entity tracking is on
                if (reference.Entity.IsEntityStateTrackingOn) ModificationEntity.TrySetPristineEntity(reference.Entity.Value);

                if (OnAfterSave != null) OnAfterSave(reference.Entity.Value, reference.Entity.UpdateType);
            }

            return saves.Any(w => w != UpdateType.Skip);
        }

        #endregion
    }
}
