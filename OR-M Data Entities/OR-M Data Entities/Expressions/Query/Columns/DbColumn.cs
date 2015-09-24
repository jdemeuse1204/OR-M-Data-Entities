/*
 * OR-M Data Entities v2.3
 * License: The MIT License (MIT)
 * Code: https://github.com/jdemeuse1204/OR-M-Data-Entities
 * Email: james.demeuse@gmail.com
 * Copyright (c) 2015 James Demeuse
 */

using System;
using System.Reflection;
using OR_M_Data_Entities.Enumeration;
using OR_M_Data_Entities.Expressions.Query.Tables;
using OR_M_Data_Entities.Mapping;

namespace OR_M_Data_Entities.Expressions.Query.Columns
{
    public class DbColumn : SimpleColumn
    {
        public DbColumn(Guid expressionQueryId, Type tableType, PropertyInfo property, string tableAlias, string parentPropertyName, bool isPrimaryKey, int ordinal)
            : base(expressionQueryId, tableType, property, tableAlias, isPrimaryKey)
        {
            Ordinal = ordinal;
            NewProperty = property;
            NewTable = new DbTable(expressionQueryId, tableType);
            IsSelected = true;
            ParentPropertyName = parentPropertyName;
        }

        public int? Order { get; set; }

        public OrderType OrderType { get; set; }

        public int Ordinal { get; set; }

        public bool IsSelected { get; set; }

        /// <summary>
        /// If its a foreign key then we need to store the name of the parent
        /// property so the loader knows which ordinals to load.  If we have 
        /// User EditedBy and User CreatedBy we need to differentiate the two when loading.
        /// If we save the parent it comes from, IE - CreatedBy, EditedBy we will be able to select the correct ordinals
        /// </summary>
        public readonly string ParentPropertyName;

        public MemberInfo NewProperty { get; set; }

        private string _newPropertyName;
        public string NewPropertyName
        {
            get
            {
                if (!string.IsNullOrWhiteSpace(_newPropertyName) || NewProperty == null) return _newPropertyName;
                var columnAttribute = NewProperty.GetCustomAttribute<ColumnAttribute>();

                _newPropertyName = columnAttribute == null ? NewProperty.Name : columnAttribute.Name;

                return _newPropertyName;
            }
        }

        public bool IsNewPropertyList
        {
            get { return NewProperty != null && NewProperty.IsList(); }
        }

        public DbTable NewTable { get; set; }
    }
}
