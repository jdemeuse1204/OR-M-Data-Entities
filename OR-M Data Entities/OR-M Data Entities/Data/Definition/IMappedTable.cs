/*
 * OR-M Data Entities v3.0
 * License: The MIT License (MIT)
 * Code: https://github.com/jdemeuse1204/OR-M-Data-Entities
 * Email: james.demeuse@gmail.com
 * Copyright (c) 2016 James Demeuse
 */

using System.Collections.Generic;

namespace OR_M_Data_Entities.Data.Definition
{

    public interface IMappedTable
    {
        string Alias { get; }

        string Key { get; } // either table name or FK/PSK property name

        ITable Table { get; }

        bool IsNullableOrListJoin { get; }

        // all selected columns
        HashSet<ISelectedColumn> SelectedColumns { get; }

        HashSet<ISelectedColumn> OrderByColumns { get; }

        HashSet<ITableRelationship> Relationships { get; }

        bool IsIncluded { get; }

        void Clear();

        void Select(string propertyName, int ordinal);

        int SelectAll(int startingOrdinal);

        void Include();

        void Exclude();

        void OrderByColumn(string propertyName);

        void OrderByPrimaryKeys();

        int MaxOrdinal();

        IMappedTable OrderByPrimaryKeysInline();

        bool HasColumn(string propertyName);
    }
}
