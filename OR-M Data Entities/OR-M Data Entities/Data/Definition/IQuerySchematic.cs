/*
 * OR-M Data Entities v3.0
 * License: The MIT License (MIT)
 * Code: https://github.com/jdemeuse1204/OR-M-Data-Entities
 * Email: james.demeuse@gmail.com
 * Copyright (c) 2014 James Demeuse
 */

using System;
using System.Collections.Generic;
using OR_M_Data_Entities.Configuration;

namespace OR_M_Data_Entities.Data.Definition
{
    public interface IQuerySchematic
    {
        Type Key { get; }

        List<IMappedTable> MappedTables { get; }

        IDataLoadSchematic DataLoadSchematic { get; }

        IConfigurationOptions ConfigurationOptions { get; }

        IMappedTable FindTable(Type type);

        IMappedTable FindTable(string tableKey);

        bool AreForeignKeysSelected();

        void Clear();
    }
}
