/*
 * OR-M Data Entities v3.0
 * License: The MIT License (MIT)
 * Code: https://github.com/jdemeuse1204/OR-M-Data-Entities
 * Email: james.demeuse@gmail.com
 * Copyright (c) 2014 James Demeuse
 */
using System;
using System.Reflection;
using OR_M_Data_Entities.Expressions.Query.Tables;
using OR_M_Data_Entities.Expressions.Resolution.Base;
using OR_M_Data_Entities.Mapping;

namespace OR_M_Data_Entities.Expressions.Query.Columns
{
    public class PartialColumn 
    {
        public PartialColumn(Type tableType, PropertyInfo property)
        {
            Table = new DbTable(tableType);
            Property = property;
        }

        public PartialColumn(Type tableType, string propertyName)
        {
            Table = new DbTable(tableType);
            _propertyName = propertyName;

            Property = tableType.GetProperty(propertyName);
        }

        public DbTable Table { get; set; }

        public MemberInfo Property { get; set; }

        public bool IsPropertyList
        {
            get { return Property != null && Property.IsList(); }
        }

        private string _propertyName;
        public string PropertyName
        {
            get
            {
                if (!string.IsNullOrWhiteSpace(_propertyName) || Property == null) return _propertyName;

                var columnAttribute = Property.GetCustomAttribute<ColumnAttribute>();

                _propertyName = columnAttribute == null ? Property.Name : columnAttribute.Name;

                return _propertyName;
            }
        }
    }
}
