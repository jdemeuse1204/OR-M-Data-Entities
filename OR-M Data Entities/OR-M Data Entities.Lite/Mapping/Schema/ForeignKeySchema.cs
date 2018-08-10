using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OR_M_Data_Entities.Lite.Mapping.Schema
{
    public class ForeignKeySchema
    {
        public ForeignKeyAttribute Attribute { get; set; }
        public Type ParentType { get; set; }
        public Type ChildType { get; set; }
        public bool MustLeftJoin { get; set; }
        public bool IsNullableKey { get; set; }
    }
}
