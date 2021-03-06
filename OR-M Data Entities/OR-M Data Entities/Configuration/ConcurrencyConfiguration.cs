﻿/*
 * OR-M Data Entities v3.0
 * License: The MIT License (MIT)
 * Code: https://github.com/jdemeuse1204/OR-M-Data-Entities
 * Email: james.demeuse@gmail.com
 * Copyright (c) 2016 James Demeuse
 */

namespace OR_M_Data_Entities.Configuration
{
    public sealed class ConcurrencyConfiguration
    {
        public bool IsOn { get; set; }

        public ConcurrencyViolationRule ViolationRule { get; set; }
    }
}
