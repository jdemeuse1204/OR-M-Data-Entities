using System.Collections.Generic;
using System.Data;
using OR_M_Data_Entities.Data.Definition;
using OR_M_Data_Entities.Data.Modification;
using OR_M_Data_Entities.Data.Query;
using OR_M_Data_Entities.Data.Secure;
using OR_M_Data_Entities.Exceptions;
using OR_M_Data_Entities.Mapping;

namespace OR_M_Data_Entities.Data
{
    public partial class DatabaseModifiable
    {
        #region Builders
        private class SqlTransactionInsertBuilder : SqlExecutionPlan
        {
            public SqlTransactionInsertBuilder(ModificationEntity entity)
                : base(entity)
            {
            }

            public override ISqlPackage Build()
            {
                return new SqlTransactionInsertPackage(this);
            }
        }
        #endregion

        #region Packages
        private class SqlTransactionInsertPackage : SqlModificationPackage
        {
            #region Constructor
            public SqlTransactionInsertPackage(ISqlBuilder builder)
                : base(builder)
            {
            }

            public SqlTransactionInsertPackage(ISqlBuilder builder, List<SqlSecureQueryParameter> parameters)
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

                var tableVariable = Entity.TableVariable;

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
        #endregion
    }
}
