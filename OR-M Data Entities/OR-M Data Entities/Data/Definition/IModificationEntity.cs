/*
 * OR-M Data Entities v3.0
 * License: The MIT License (MIT)
 * Code: https://github.com/jdemeuse1204/OR-M-Data-Entities
 * Email: james.demeuse@gmail.com
 * Copyright (c) 2016 James Demeuse
 */

using System.Collections.Generic;
using OR_M_Data_Entities.Configuration;
using OR_M_Data_Entities.Data.Modification;

namespace OR_M_Data_Entities.Data.Definition
{
    public interface IModificationEntity : IEntity
    {
        UpdateType UpdateType { get; }

        EntityState State { get; }

        IReadOnlyList<IModificationItem> Changes();

        IReadOnlyList<IModificationItem> Keys();

        IReadOnlyList<IModificationItem> All();

        void CalculateChanges(IConfigurationOptions configuration);
    }
}
