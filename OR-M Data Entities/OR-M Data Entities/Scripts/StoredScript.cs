/*
 * OR-M Data Entities v3.1
 * License: The MIT License (MIT)
 * Code: https://github.com/jdemeuse1204/OR-M-Data-Entities
 * Email: james.demeuse@gmail.com
 * Copyright (c) 2016 James Demeuse
 */

using OR_M_Data_Entities.Scripts.Base;

namespace OR_M_Data_Entities.Scripts
{
    public abstract class StoredScript<T> : StoredScript, IReadScript<T>
    {
    }

    public abstract class StoredScript : IWriteScript
    {
    }
}
