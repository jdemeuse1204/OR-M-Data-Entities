/*
 * OR-M Data Entities v2.0
 * License: The MIT License (MIT)
 * Code: https://github.com/jdemeuse1204/OR-M-Data-Entities
 * Copyright (c) 2015 James Demeuse
 */
using System;

namespace OR_M_Data_Entities.Expressions.Query.Tables
{
    public class AliasTable : DbTable
    {
        public AliasTable(Guid expressionQueryId, Type type, string alias)
            : base(expressionQueryId, type)
        {
            Alias = alias;
        }

        public string Alias { get; set; }
    }
}
