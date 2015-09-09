/*
 * OR-M Data Entities v2.2
 * License: The MIT License (MIT)
 * Code: https://github.com/jdemeuse1204/OR-M-Data-Entities
 * Email: james.demeuse@gmail.com
 * Copyright (c) 2015 James Demeuse
 */

using System.Collections.Generic;
using OR_M_Data_Entities.Enumeration;

namespace OR_M_Data_Entities.Tracking
{
    public class EntityStateComparePackage
    {
        public EntityStateComparePackage(EntityState state, IEnumerable<string> changeList)
        {
            State = state;
            ChangeList = changeList;
        }

        public readonly EntityState State;

        public readonly IEnumerable<string> ChangeList;
    }
}
