/*
 * OR-M Data Entities v2.0
 * License: The MIT License (MIT)
 * Code: https://github.com/jdemeuse1204/OR-M-Data-Entities
 * Copyright (c) 2015 James Demeuse
 */
using System;
using System.Collections.Generic;
using OR_M_Data_Entities.Data.Definition.Base;

namespace OR_M_Data_Entities.Expressions.Query.Tables
{
    public class ForeignKeyTable : AliasTable
    {
        public ForeignKeyTable(Guid expressionQueryId, Type type, string foreignKeyTableName,  string alias = "")
            : base(expressionQueryId, type, alias)
        {
            TableInfo = new TableInfo(type);
            ForeignKeyPropertyName = foreignKeyTableName;
            _typeChanges = new List<Type>();
        }

        public readonly TableInfo TableInfo;

        public readonly string ForeignKeyPropertyName;
         
        private readonly List<Type> _typeChanges;

        public IEnumerable<Type> TypeChanges { get { return _typeChanges; } }

        public void ChangeType(Type newType)
        {
            if (!_typeChanges.Contains(newType)) _typeChanges.Add(newType);
        }
    }
}
