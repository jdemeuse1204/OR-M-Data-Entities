/*
 * OR-M Data Entities v3.0
 * License: The MIT License (MIT)
 * Code: https://github.com/jdemeuse1204/OR-M-Data-Entities
 * Email: james.demeuse@gmail.com
 * Copyright (c) 2016 James Demeuse
 */

using System;

namespace OR_M_Data_Entities.Exceptions
{
    public class DataLoadException : Exception
    {
        public DataLoadException(string message, Exception innerException)
            : base(message, innerException)
        {

        }
    }
}
