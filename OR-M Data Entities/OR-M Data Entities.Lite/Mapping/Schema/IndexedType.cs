using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OR_M_Data_Entities.Lite.Mapping.Schema
{
    internal class IndexedType
    {
        public IndexedType(Type type, string propertyName = "")
        {
            Type = type;
            PropertyName = propertyName;
        }

        public Type Type { get; }
        
        public string PropertyName { get; }

        public override bool Equals(object obj)
        {
            return obj != null && ((IndexedType)obj).Type == Type && ((IndexedType)obj).PropertyName == PropertyName;
        }
    }
}
