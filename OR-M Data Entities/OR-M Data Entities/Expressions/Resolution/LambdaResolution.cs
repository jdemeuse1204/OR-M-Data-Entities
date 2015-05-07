using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using OR_M_Data_Entities.Enumeration;

namespace OR_M_Data_Entities.Expressions.Resolution
{
    public class LambdaResolution
    {
        public LambdaResolution()
        {
            TableName = string.Empty;
            ColumnName = string.Empty;
            Comparison = CompareType.None;
            Group = -1;
        }

        public string TableName { get; set; }

        public string ColumnName { get; set; }

        public CompareType Comparison { get; set; }

        public object CompareValue { get; set; }

        public int Group { get; set; }
    }
}
