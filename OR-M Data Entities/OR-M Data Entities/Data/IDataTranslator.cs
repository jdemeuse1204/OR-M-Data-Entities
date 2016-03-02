/*
 * OR-M Data Entities v3.0
 * License: The MIT License (MIT)
 * Code: https://github.com/jdemeuse1204/OR-M-Data-Entities
 * Email: james.demeuse@gmail.com
 * Copyright (c) 2016 James Demeuse
 */

using System;
using System.Collections;
using System.Collections.Generic;

namespace OR_M_Data_Entities.Data
{
    public interface IDataTranslator<T> : IEnumerable, IDisposable
    {
        bool HasRows { get; }

        T FirstOrDefault();

        T First();

        List<T> ToList();
    }
}
