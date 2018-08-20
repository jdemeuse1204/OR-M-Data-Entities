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
        public TableSchema(string name)
        {
            Name = name;
        }

        public string Name { get; }
        public bool HasKeysOnly { get; set; }
        public int KeyCount { get; set; }
        public IReadOnlyList<ColumnSchema> Columns { get; set; }
    }
}
