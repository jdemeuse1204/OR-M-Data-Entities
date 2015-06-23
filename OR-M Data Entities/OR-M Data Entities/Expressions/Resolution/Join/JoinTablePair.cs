﻿/*
 * OR-M Data Entities v2.0
 * License: The MIT License (MIT)
 * Code: https://github.com/jdemeuse1204/OR-M-Data-Entities
 * Copyright (c) 2015 James Demeuse
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

        // FK or PK property name from the parent
        public string ParentJoinPropertyName { get; private set; }
        #endregion

        #region Constructor
        public JoinTablePair(ForeignKeyTable parentTable, ForeignKeyTable childTable, bool heirarchyContainsList)
        {
            ChildTable = childTable;
            ParentTable = parentTable;
            HeirarchyContainsList = heirarchyContainsList;
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