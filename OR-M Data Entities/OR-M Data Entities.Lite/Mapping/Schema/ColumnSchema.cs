using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OR_M_Data_Entities.Lite.Mapping.Schema
{
    internal class ColumnSchema
    {
        public string PropertyName { get; set; }
        public string ColumnName { get; set; }
        public ForeignKeySchema ForeignKey { get; set; }
        public bool IsKey { get; set; }
    }
}
