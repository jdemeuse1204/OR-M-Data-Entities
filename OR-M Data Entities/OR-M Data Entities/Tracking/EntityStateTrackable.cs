﻿/*
 * OR-M Data Entities v3.0
 * License: The MIT License (MIT)
 * Code: https://github.com/jdemeuse1204/OR-M-Data-Entities
 * Email: james.demeuse@gmail.com
 * Copyright (c) 2016 James Demeuse
 */

using OR_M_Data_Entities.Data;

namespace OR_M_Data_Entities.Tracking
{
    public abstract class EntityStateTrackable
    {
        private object _pristineEntity;

        public EntityState GetState()
        {
            return DatabaseSchematic.GetEntityState(this);
        }
    }
}
