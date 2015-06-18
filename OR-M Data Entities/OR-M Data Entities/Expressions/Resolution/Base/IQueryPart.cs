/*
 * OR-M Data Entities v2.0
 * License: The MIT License (MIT)
 * Code: https://github.com/jdemeuse1204/OR-M-Data-Entities
 * Copyright (c) 2015 James Demeuse
 */
using System;

namespace OR_M_Data_Entities.Expressions.Resolution.Base
{
    public interface IQueryPart
    {
        Guid ExpressionQueryId { get; set; }
    }
}
