/*
 * OR-M Data Entities v2.2
 * License: The MIT License (MIT)
 * Code: https://github.com/jdemeuse1204/OR-M-Data-Entities
 * Email: james.demeuse@gmail.com
 * Copyright (c) 2015 James Demeuse
 */

using OR_M_Data_Entities.Enumeration;

namespace OR_M_Data_Entities.Tracking
{
    public abstract class EntityStateTrackable
    {
        private object _pristineEntity;

        public EntityState GetState()
        {
            var package = EntityStateAnalyzer.Analyze(this);

            return package.State;
        }
    }
}
