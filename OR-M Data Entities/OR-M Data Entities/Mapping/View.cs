﻿/*
 * OR-M Data Entities v2.3
 * License: The MIT License (MIT)
 * Code: https://github.com/jdemeuse1204/OR-M-Data-Entities
 * Email: james.demeuse@gmail.com
 * Copyright (c) 2014 James Demeuse
 */

using System;

namespace OR_M_Data_Entities.Mapping
{
    [AttributeUsage(AttributeTargets.Class, AllowMultiple = false)]
    public class ViewAttribute : Attribute
    {
        public ViewAttribute(params string[] viewIds)
        {
            ViewIds = viewIds;
        }

        public string[] ViewIds { get; private set; }
    }
}