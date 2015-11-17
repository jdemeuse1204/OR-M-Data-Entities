﻿/*
 * OR-M Data Entities v3.0
 * License: The MIT License (MIT)
 * Code: https://github.com/jdemeuse1204/OR-M-Data-Entities
 * Email: james.demeuse@gmail.com
 * Copyright (c) 2014 James Demeuse
 */
using System;

namespace OR_M_Data_Entities.Expressions.Resolution.Base
{
    public abstract class ResolutionContainerBase
    {
        public readonly Guid ExpressionQueryId;

        protected ResolutionContainerBase(Guid expressionQueryId)
        {
            ExpressionQueryId = expressionQueryId;
        }
    }
}
