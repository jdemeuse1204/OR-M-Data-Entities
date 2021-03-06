﻿/*
 * OR-M Data Entities v3.0
 * License: The MIT License (MIT)
 * Code: https://github.com/jdemeuse1204/OR-M-Data-Entities
 * Email: james.demeuse@gmail.com
 * Copyright (c) 2016 James Demeuse
 */

using System.Data;
using OR_M_Data_Entities.Mapping;

namespace OR_M_Data_Entities.Data.Modification
{
    public interface IModificationItem
    {
        bool IsModified { get; }

        string SqlDataTypeString { get; }

        string PropertyDataType { get; }

        string PropertyName { get; }

        string DatabaseColumnName { get; }

        SqlDbType DbTranslationType { get; }

        bool IsPrimaryKey { get; }

        DbGenerationOption Generation { get; }

        string GetUniqueKey();

        bool TranslateDataType { get; }

        bool NeedsAlias { get; }

        string AsOutput(string appendToEnd);

        string AsField(string appendToEnd);

        string AsFieldPropertyName(string appendToEnd);
    }
}
