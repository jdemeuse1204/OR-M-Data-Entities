/*
 * OR-M Data Entities v2.3
 * License: The MIT License (MIT)
 * Code: https://github.com/jdemeuse1204/OR-M-Data-Entities
 * Email: james.demeuse@gmail.com
 * Copyright (c) 2014 James Demeuse
 */

using System;

namespace OR_M_Data_Entities.Mapping
{
    /// <summary>
    /// Mark any lookup table with this attribute so any item from a lookup table that is being used as a foreign key will not be deleted.
    /// To delete an item from a lookup table you must delete the base object.
    /// </summary>
    [AttributeUsage(AttributeTargets.Class, AllowMultiple = false)]
    public sealed class LookupTableAttribute : TableAttribute
    {
        public LookupTableAttribute(string name) : 
            base(name)
        {
        }
    }
}
