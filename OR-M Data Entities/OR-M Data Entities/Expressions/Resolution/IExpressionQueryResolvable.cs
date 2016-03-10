/*
 * OR-M Data Entities v2.3
 * License: The MIT License (MIT)
 * Code: https://github.com/jdemeuse1204/OR-M-Data-Entities
 * Email: james.demeuse@gmail.com
 * Copyright (c) 2014 James Demeuse
 */

using System;
using System.Collections.Generic;
using OR_M_Data_Entities.Data;
using OR_M_Data_Entities.Data.Definition;
using OR_M_Data_Entities.Enumeration;
using OR_M_Data_Entities.Expressions.Collections;
using OR_M_Data_Entities.Expressions.Query.Columns;

namespace OR_M_Data_Entities.Expressions.Resolution
{
    public interface IExpressionQueryResolvable
    {
        void ResolveExpression();

        IEnumerable<SqlDbParameter> Parameters { get; }

        ReadOnlyTableCollection Tables { get; }

        DatabaseReading DbContext { get; }

        // for use with payload
        IEnumerable<DbColumn> SelectInfos { get; }

        void Clear();

        bool IsSubQuery { get; }

        OSchematic LoadSchematic { get; }

        bool HasForeignKeys { get; }

        string Sql { get; }

        void Initialize();

        Guid Id { get; }

        ExpressionQueryConstructionType ConstructionType { get; }

        int GetOrdinalBySelectedColumns(int oldOrdinal);
    }
}
