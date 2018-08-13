using System;
using System.Collections.Generic;

namespace OR_M_Data_Entities.Lite.Mapping.Schema
{
    public class MappedType
    {
        public MappedType(Type type, int[][] location)
        {
            Type = type;
            WasRead = false;
        }

        public Type Type { get; }
        public int[][] Location { get; }
        public bool WasRead { get; set; }
        public List<MappedType> Dependencies { get; set; }
    }
}
