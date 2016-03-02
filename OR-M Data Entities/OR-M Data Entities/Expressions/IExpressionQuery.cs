/*
 * OR-M Data Entities v3.0
 * License: The MIT License (MIT)
 * Code: https://github.com/jdemeuse1204/OR-M-Data-Entities
 * Email: james.demeuse@gmail.com
 * Copyright (c) 2016 James Demeuse
 */

using System.Collections.Generic;

namespace OR_M_Data_Entities.Expressions
{
    public interface IExpressionQuery<out TSource> : IEnumerable<TSource>
    {
        string Sql();
    }
}
