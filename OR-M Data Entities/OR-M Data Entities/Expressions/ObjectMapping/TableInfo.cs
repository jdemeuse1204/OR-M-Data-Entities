using System;
using System.Collections.Generic;
using System.Reflection;

namespace OR_M_Data_Entities.Expressions.ObjectMapping
{
    public class TableInfo
    {
        public TableInfo()
        {
            KeyHashesLoaded = new List<int>();
        }

        public string TableName { get; set; }

        public string TableAlias { get; set; }

        public Type Type { get; set; }

        public bool IsList { get; set; }

        public string[] PrimaryKeys { get; set; }

        public Type ParentType { get; set; }

        public PropertyInfo Property { get; set; }

        public PropertyInfo ParentProperty { get; set; }

        public List<int> KeyHashesLoaded { get; set; } 
    }
}
