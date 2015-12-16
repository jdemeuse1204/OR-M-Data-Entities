/*
 * OR-M Data Entities v2.3
 * License: The MIT License (MIT)
 * Code: https://github.com/jdemeuse1204/OR-M-Data-Entities
 * Email: james.demeuse@gmail.com
 * Copyright (c) 2014 James Demeuse
 */

using System;
using OR_M_Data_Entities.Expressions.Query.Tables;

namespace OR_M_Data_Entities.Expressions.Resolution.Join
{
    public class JoinTablePair
    {
        #region Properties
        public bool HeirarchyContainsList { get; private set; }

        public ForeignKeyTable ChildTable { get; private set; }

        public ForeignKeyTable ParentTable { get; private set; }

        public bool IsLinkedServerJoin { get; private set; }

        // FK or PK property name from the parent
        public string ParentJoinPropertyName { get; private set; }
        #endregion

        #region Constructor
        public JoinTablePair(ForeignKeyTable parentTable, ForeignKeyTable childTable, bool heirarchyContainsList)
        {
            ChildTable = childTable;
            ParentTable = parentTable;
            HeirarchyContainsList = heirarchyContainsList;
            IsLinkedServerJoin = childTable.TableInfo.IsUsingLinkedServer;
        }

        public JoinTablePair(Guid expressionQueryId, Type parentType, Type childType, bool heirarchyContainsList, string computedParentAlias,
            string computedChildAlias, string parentPropertyName, string childPropertyName, string parentJoinPropertyName)
        {
            ChildTable = new ForeignKeyTable(expressionQueryId, childType, childPropertyName, computedChildAlias);
            ParentTable = new ForeignKeyTable(expressionQueryId, parentType, parentPropertyName,  computedParentAlias);
            HeirarchyContainsList = heirarchyContainsList;
            ParentJoinPropertyName = parentJoinPropertyName;
        }

        #endregion
    }
}
