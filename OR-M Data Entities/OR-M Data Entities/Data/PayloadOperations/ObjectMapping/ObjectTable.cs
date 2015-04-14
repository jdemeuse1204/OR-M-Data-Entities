using System;
using System.Collections.Generic;
using System.Reflection;

namespace OR_M_Data_Entities.Data.PayloadOperations.ObjectMapping
{
    public class ObjectTable
    {
        public Type Type { get; set; }

        public string Alias { get; set; }

        public string TableName { get; set; }

        public List<PropertyInfo> Columns { get; set; } 
    }
}
