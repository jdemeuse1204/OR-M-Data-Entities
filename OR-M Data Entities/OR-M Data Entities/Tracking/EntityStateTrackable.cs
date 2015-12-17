/*
 * OR-M Data Entities v3.0
 * License: The MIT License (MIT)
 * Code: https://github.com/jdemeuse1204/OR-M-Data-Entities
 * Email: james.demeuse@gmail.com
 * Copyright (c) 2014 James Demeuse
 */
using OR_M_Data_Entities.Data.Definition;
using OR_M_Data_Entities.Enumeration;

namespace OR_M_Data_Entities.Tracking
{
    public abstract class EntityStateTrackable
    {
        private object _pristineEntity;

        public EntityState GetState()
        {
            return ModificationEntity.GetState(this);
        }
    }
}
