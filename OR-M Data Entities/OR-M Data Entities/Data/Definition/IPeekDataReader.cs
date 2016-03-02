/*
 * OR-M Data Entities v3.0
 * License: The MIT License (MIT)
 * Code: https://github.com/jdemeuse1204/OR-M-Data-Entities
 * Email: james.demeuse@gmail.com
 * Copyright (c) 2016 James Demeuse
 */

using System.Data;

namespace OR_M_Data_Entities.Data.Definition
{
    public interface IPeekDataReader : IDataReader
    {
        bool WasPeeked { get; }

        dynamic ToDynamic();

        bool HasRows { get; }

        T ToObjectDefault<T>();

        T ToObject<T>();

        bool Peek();
    }
}
