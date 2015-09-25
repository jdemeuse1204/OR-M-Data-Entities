﻿/*
 * OR-M Data Entities v2.3
 * License: The MIT License (MIT)
 * Code: https://github.com/jdemeuse1204/OR-M-Data-Entities
 * Email: james.demeuse@gmail.com
 * Copyright (c) 2015 James Demeuse
 */

using System;

namespace OR_M_Data_Entities.Exceptions
{
    public class SqlReferenceConstraintException : Exception
    {
        public SqlReferenceConstraintException(string message)
            : base(message)
        {

        }
    }
}