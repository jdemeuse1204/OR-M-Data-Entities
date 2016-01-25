﻿/*
 * OR-M Data Entities v3.0
 * License: The MIT License (MIT)
 * Code: https://github.com/jdemeuse1204/OR-M-Data-Entities
 * Email: james.demeuse@gmail.com
 * Copyright (c) 2014 James Demeuse
 */

using System;

namespace OR_M_Data_Entities.Expressions.Query.Tables
{
    public class AliasTable : DbTable
    {
        public AliasTable(Type type, string alias)
            : base(type)
        {
            Alias = alias;
        }

        public string Alias { get; set; }
    }
}
