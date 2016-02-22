/*
 * OR-M Data Entities v3.0
 * License: The MIT License (MIT)
 * Code: https://github.com/jdemeuse1204/OR-M-Data-Entities
 * Email: james.demeuse@gmail.com
 * Copyright (c) 2014 James Demeuse
 */

namespace OR_M_Data_Entities.Data.Definition
{
    public interface IAutoLoadKeyRelationship
    {
        IColumn ChildColumn { get; }

        IColumn ParentColumn { get; }

        IColumn AutoLoadPropertyColumn { get; }

        JoinType JoinType { get; }

        bool IsNullableOrListJoin { get; }
    }
}