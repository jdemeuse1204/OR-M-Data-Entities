using System;
using System.Collections.Generic;

namespace OR_M_Data_Entities.Data.PayloadOperations.ObjectMapping
{
    public class ObjectTable
    {
        public Type Type { get; set; }

        public string Alias { get; set; }

        public string TableName { get; set; }

        public List<ObjectColumn> Columns { get; set; }

        public bool HasAlias { get { return TableName == null ? TableName == Alias : TableName.Equals(Alias); } }
    }
}
