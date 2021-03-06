﻿/*
 * OR-M Data Entities v3.0
 * License: The MIT License (MIT)
 * Code: https://github.com/jdemeuse1204/OR-M-Data-Entities
 * Email: james.demeuse@gmail.com
 * Copyright (c) 2016 James Demeuse
 */

using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using OR_M_Data_Entities.Configuration;
using OR_M_Data_Entities.Data.Loading;

namespace OR_M_Data_Entities.Data.Definition
{
    public interface IQuerySchematic
    {
        Type Key { get; }

        Expression ReturnOverride { get; }

        List<IMappedTable> MappedTables { get; }

        IDataLoadSchematic DataLoadSchematic { get; }

        IConfigurationOptions ConfigurationOptions { get; }

        IMappedTable FindTable(Type type);

        IMappedTable FindTable(string tableKey);

        bool HasTable(Type type);

        bool HasTable(string tableKey);

        bool AreForeignKeysSelected();

        void InitializeSelect(bool isLazyLoading);

        int NextOrdinal();

        void UnSelectAll();

        void SelectAll();

        void ClearReadCache();

        void Reset();

        void SetReturnOverride(Expression expression);
    }
}
