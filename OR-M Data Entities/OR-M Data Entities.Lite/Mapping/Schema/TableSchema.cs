using FastMember;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OR_M_Data_Entities.Lite.Mapping.Schema
{
    internal class TableSchema
    {
        public TableSchema(string name, int index)
        {
            Name = name;
            Index = index;
        }

        public TypeAccessor TypeAccessor { get; set; }
        public string Name { get; }
        public IEnumerable<ColumnSchema> Columns { get; set; }
        public int Index { get; }
    }
}
