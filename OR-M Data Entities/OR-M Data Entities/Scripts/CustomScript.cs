/*
 * OR-M Data Entities v3.0
 * License: The MIT License (MIT)
 * Code: https://github.com/jdemeuse1204/OR-M-Data-Entities
 * Email: james.demeuse@gmail.com
 * Copyright (c) 2014 James Demeuse
 */
using OR_M_Data_Entities.Scripts.Base;

namespace OR_M_Data_Entities.Scripts
{
    public abstract class CustomScript<T> : CustomScript, IReadScript<T>
    {
    }

    public abstract class CustomScript : IWriteScript
    {
        protected abstract string Sql { get; }

        public string GetSql()
        {
            return Sql;
        }
    }
}
