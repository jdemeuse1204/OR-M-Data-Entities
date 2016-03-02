/*
 * OR-M Data Entities v3.0
 * License: The MIT License (MIT)
 * Code: https://github.com/jdemeuse1204/OR-M-Data-Entities
 * Email: james.demeuse@gmail.com
 * Copyright (c) 2016 James Demeuse
 */

using System;
using System.Collections.Generic;
using OR_M_Data_Entities.Data.Loading;

namespace OR_M_Data_Entities.Data.Definition
{
    public interface IDataLoadSchematic
    {
        HashSet<IDataLoadSchematic> Children { get; }

        IDataLoadSchematic Parent { get; }

        Type ActualType { get; }

        string[] PrimaryKeyNames { get; }

        HashSet<CompositeKey> LoadedCompositePrimaryKeys { get; }

        object ReferenceToCurrent { get; set; }

        IMappedTable MappedTable { get; }

        /// <summary>
        /// used to identity Foreign Key because object can have Foreign Key with same type,
        /// but load different data.  IE - User CreatedBy, User EditedBy
        /// </summary>
        string PropertyName { get; }

        Type Type { get; }

        void ClearRowReadCache();

        void ClearLoadedCompositePrimaryKeys();
    }
}
