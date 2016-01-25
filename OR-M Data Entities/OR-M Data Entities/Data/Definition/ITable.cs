﻿/*
 * OR-M Data Entities v3.0
 * License: The MIT License (MIT)
 * Code: https://github.com/jdemeuse1204/OR-M-Data-Entities
 * Email: james.demeuse@gmail.com
 * Copyright (c) 2014 James Demeuse
 */

using System;
using System.Collections.Generic;
using System.Reflection;
using OR_M_Data_Entities.Mapping;

namespace OR_M_Data_Entities.Data.Definition
{
    public interface ITable
    {
        Type Type { get; }

        bool IsReadOnly { get; }

        bool IsUsingLinkedServer { get; }

        bool IsLookupTable { get; }

        string ClassName { get; }

        string ServerName { get; }

        string DatabaseName { get; }

        string SchemaName { get; }

        bool IsEntityStateTrackingOn { get; }

        string ToString(TableNameFormat format);

        bool IsPrimaryKey(string columnName);

        bool IsPrimaryKey(PropertyInfo property);

        bool HasForeignKeys();

        bool HasPrimaryKeysOnly();

        List<PropertyInfo> GetAllColumns();

        List<PropertyInfo> GetAllProperties();

        List<PropertyInfo> GetAllForeignAndPseudoKeys(string viewId = null);

        string GetColumnName(string propertyName);

        ReadOnlySaveOption? GetReadOnlySaveOption();

        List<PropertyInfo> GetPrimaryKeys();
    }
}
