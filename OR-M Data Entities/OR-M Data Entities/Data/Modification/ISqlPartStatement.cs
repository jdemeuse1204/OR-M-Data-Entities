﻿/*
 * OR-M Data Entities v3.0
 * License: The MIT License (MIT)
 * Code: https://github.com/jdemeuse1204/OR-M-Data-Entities
 * Email: james.demeuse@gmail.com
 * Copyright (c) 2016 James Demeuse
 */

namespace OR_M_Data_Entities.Data.Modification
{
    public interface ISqlPartStatement
    {
        string Sql { get; }

        string Declare { get; }

        string Set { get; }

        string ToString();
    }
}
