﻿/*
 * OR-M Data Entities v2.0
 * License: The MIT License (MIT)
 * Code: https://github.com/jdemeuse1204/OR-M-Data-Entities
 * Copyright (c) 2015 James Demeuse
 */
using System;
using OR_M_Data_Entities.Data.Definition;
using OR_M_Data_Entities.Expressions.Resolution.Base;

namespace OR_M_Data_Entities.Expressions.Query.Tables
{
    public class DbTable : IQueryPart
    {
        public DbTable(Guid expressionQueryId, Type type)
        {
            ExpressionQueryId = expressionQueryId;
            Type = type;
        }

        public Guid ExpressionQueryId { get; set; }

        public Type Type { get; set; }

        private string _name;
        public string Name
        {
            get
            {
                if (string.IsNullOrWhiteSpace(_name) && Type != null)
                {
                    _name = DatabaseSchemata.GetTableName(Type);
                }
                return _name;
            }
        }
    }
}
