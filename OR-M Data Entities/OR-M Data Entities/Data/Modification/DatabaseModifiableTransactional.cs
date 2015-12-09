using System.Collections.Generic;
using OR_M_Data_Entities.Data.Definition;
using OR_M_Data_Entities.Data.Modification;
using OR_M_Data_Entities.Data.Query;
using OR_M_Data_Entities.Data.Secure;

namespace OR_M_Data_Entities.Data
{
    public partial class DatabaseModifiable
    {
        #region Builders
        private class SqlTransactionInsertBuilder : SqlExecutionPlan
        {
            private readonly EntitySaveNodeList _nodes;

            private readonly List<SqlSecureQueryParameter> _parameters;

            public SqlTransactionInsertBuilder(ModificationEntity entity, EntitySaveNodeList nodes, List<SqlSecureQueryParameter> parameters)
                : base(entity)
            {
                _parameters = parameters;
                _nodes = nodes;
            }

            public override ISqlPackage Build()
            {
                return new SqlTransactionInsertPackage(this, _parameters, _nodes);
            }
        }
        #endregion

        #region Packages
        private class SqlTransactionInsertPackage : SqlInsertPackage
        {
            #region Constructor
            public SqlTransactionInsertPackage(ISqlBuilder builder, List<SqlSecureQueryParameter> parameters, EntitySaveNodeList nodes)
                : base(builder, parameters, nodes)
            {
                IsUsingTransactions = true;
            }
            #endregion
        }
        #endregion
    }
}
