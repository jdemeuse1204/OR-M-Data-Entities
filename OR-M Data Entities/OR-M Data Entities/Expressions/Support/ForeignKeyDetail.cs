using System;
using System.Collections.Generic;

namespace OR_M_Data_Entities.Expressions.Support
{
    public class ForeignKeyDetail
    {
        public Type ParentType { get; set; }

        public Type Type { get; set; }

        public string PropertyName { get; set; }

        public bool IsList { get; set; }

        public Type ListType { get; set; }

        public string[] PrimaryKeyDatabaseNames { get; set; }

        // <ParentHashCode, CurrentHashCode>
        public Dictionary<int, List<int>> KeysSelectedHashCodeList { get; set; }

        public List<ForeignKeyDetail> ChildTypes { get; set; }
    }
}
