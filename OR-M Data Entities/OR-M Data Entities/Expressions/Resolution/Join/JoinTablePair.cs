using System;
using OR_M_Data_Entities.Expressions.Query.Tables;

namespace OR_M_Data_Entities.Expressions.Resolution.Join
{
    public class JoinTablePair
    {
        #region Properties
        public bool HeirarchyContainsList { get; private set; }

        public ForeignKeyTable ChildTable { get; set; }

        public ForeignKeyTable ParentTable { get; set; }
        #endregion

        #region Constructor
        public JoinTablePair(ForeignKeyTable parentTable, ForeignKeyTable childTable, bool heirarchyContainsList)
        {
            ChildTable = childTable;
            ParentTable = parentTable;
            HeirarchyContainsList = heirarchyContainsList;
        }

        public JoinTablePair(Guid expressionQueryId, Type parentType, Type childType, bool heirarchyContainsList, string computedParentAlias,
            string computedChildAlias, string parentPropertyName, string childPropertyName)
        {
            ChildTable = new ForeignKeyTable(expressionQueryId, childType, childPropertyName, computedChildAlias);
            ParentTable = new ForeignKeyTable(expressionQueryId, parentType, parentPropertyName, computedParentAlias);
            HeirarchyContainsList = heirarchyContainsList;
        }

        #endregion
    }
}
