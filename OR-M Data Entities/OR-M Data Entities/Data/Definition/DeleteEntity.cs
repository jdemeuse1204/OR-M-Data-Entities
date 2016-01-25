/*
 * OR-M Data Entities v3.0
 * License: The MIT License (MIT)
 * Code: https://github.com/jdemeuse1204/OR-M-Data-Entities
 * Email: james.demeuse@gmail.com
 * Copyright (c) 2014 James Demeuse
 */

using System.Linq;
using OR_M_Data_Entities.Configuration;
using OR_M_Data_Entities.Data.Modification;

namespace OR_M_Data_Entities.Data.Definition
{
    public sealed class DeleteEntity : ModificationEntity
    {
        public DeleteEntity(object entity, ConfigurationOptions configuration) 
            : base(entity, true, configuration)
        {
            ModificationItems = GetColumns().Select(w => new ModificationItem(w)).ToList();
        }
    }
}
