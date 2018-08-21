﻿/*
 * OR-M Data Entities v3.1
 * License: The MIT License (MIT)
 * Code: https://github.com/jdemeuse1204/OR-M-Data-Entities
 * Email: james.demeuse@gmail.com
 * Copyright (c) 2016 James Demeuse
 */

namespace OR_M_Data_Entities.Expressions
{
    public interface IModificationQueryResolvable<TSource>
    {
        IExpressionQueryResolvable<TSource> Set();
    }
}