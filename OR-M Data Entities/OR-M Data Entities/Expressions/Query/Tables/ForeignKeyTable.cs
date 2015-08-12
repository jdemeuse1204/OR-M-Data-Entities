/*
 * OR-M Data Entities v2.1
 * License: The MIT License (MIT)
 * Code: https://github.com/jdemeuse1204/OR-M-Data-Entities
 * Email: james.demeuse@gmail.com
 * Copyright (c) 2015 James Demeuse
 */

using System;
using System.Collections.Generic;
using System.Reflection;
using System.Runtime.Remoting.Proxies;
using OR_M_Data_Entities.Data.Definition.Base;
using OR_M_Data_Entities.Mapping;

namespace OR_M_Data_Entities.Expressions.Query.Tables
{
    public class ForeignKeyTable : AliasTable
    {
        public ForeignKeyTable(Guid expressionQueryId, Type type, string foreignKeyTableName,  string alias = "", string parentTableAlias = "")
            : base(expressionQueryId, type, alias)
        {
            TableInfo = new TableInfo(type);
            ForeignKeyPropertyName = foreignKeyTableName;
            _typeChanges = new List<Type>();
            ParentTableAlias = parentTableAlias;
        }

        public readonly TableInfo TableInfo;

        public readonly string ParentTableAlias;

        public readonly string ForeignKeyPropertyName;
         
        private readonly List<Type> _typeChanges;

        public IEnumerable<Type> TypeChanges { get { return _typeChanges; } }

        public void ChangeType(Type newType)
        {
            if (!_typeChanges.Contains(newType)) _typeChanges.Add(newType);
        }

        public string GetForeignKeyDatabaseColumnName()
        {
            var property = this.Type.GetProperty(ForeignKeyPropertyName);
            var attribute = property.GetCustomAttribute<ColumnAttribute>();

            return attribute == null ? property.Name : attribute.Name;
        }
    }
}
